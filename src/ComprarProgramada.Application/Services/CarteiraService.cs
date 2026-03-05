using ComprarProgramada.Application.DTOs.Carteira;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.Interfaces.Services;

namespace ComprarProgramada.Application.Services;

public sealed class CarteiraService : ICarteiraService
{
    private readonly IClienteRepository _clientes;
    private readonly ICustodiaFilhoteRepository _custodias;
    private readonly ICotacaoService _cotacoes;
    private readonly IDistribuicaoRepository _distribuicoes;

    public CarteiraService(
        IClienteRepository clientes,
        ICustodiaFilhoteRepository custodias,
        ICotacaoService cotacoes,
        IDistribuicaoRepository distribuicoes)
    {
        _clientes = clientes;
        _custodias = custodias;
        _cotacoes = cotacoes;
        _distribuicoes = distribuicoes;
    }

    public async Task<CarteiraResponse> ObterCarteiraAsync(int clienteId, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(clienteId, ct)
            ?? throw new KeyNotFoundException($"Cliente {clienteId} não encontrado.");

        var contaId = cliente.ContaFilhote?.Id
            ?? throw new InvalidOperationException($"Cliente {clienteId} não possui conta gráfica.");

        var custodia = await _custodias.ObterPorContaFilhoteAsync(contaId, ct)
            ?? throw new InvalidOperationException($"Custódia não encontrada para conta {contaId}.");

        var posicoes = new List<PosicaoItem>();
        decimal valorTotalInvestido = 0;
        decimal valorAtualCarteira = 0;

        foreach (var item in custodia.Itens)
        {
            decimal precoAtual;
            try
            {
                precoAtual = await _cotacoes.ObterCotacaoFechamentoAsync(item.Ticker, ct);
            }
            catch
            {
                // Se não encontrar cotação, usa preço médio como fallback
                precoAtual = item.PrecoMedio;
            }

            var valorInvestido = item.Quantidade * item.PrecoMedio;
            var valorAtual = item.Quantidade * precoAtual;
            var lucro = valorAtual - valorInvestido;
            var rentab = valorInvestido > 0 ? (lucro / valorInvestido) * 100m : 0m;

            valorTotalInvestido += valorInvestido;
            valorAtualCarteira += valorAtual;

            posicoes.Add(new PosicaoItem(
                item.Ticker.Valor,
                item.Quantidade,
                item.PrecoMedio,
                precoAtual,
                Math.Round(valorInvestido, 2),
                Math.Round(valorAtual, 2),
                Math.Round(lucro, 2),
                Math.Round(rentab, 2)));
        }

        var rentabTotal = valorTotalInvestido > 0
            ? ((valorAtualCarteira - valorTotalInvestido) / valorTotalInvestido) * 100m
            : 0m;

        return new CarteiraResponse(
            cliente.Id,
            cliente.Nome,
            cliente.ContaFilhote!.NumeroConta,
            Math.Round(valorTotalInvestido, 2),
            Math.Round(valorAtualCarteira, 2),
            Math.Round(rentabTotal, 2),
            posicoes);
    }

    public async Task<RentabilidadeResponse> ObterRentabilidadeAsync(int clienteId, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(clienteId, ct)
            ?? throw new KeyNotFoundException($"Cliente {clienteId} não encontrado.");

        var contaId = cliente.ContaFilhote?.Id
            ?? throw new InvalidOperationException($"Cliente {clienteId} não possui conta gráfica.");

        var custodia = await _custodias.ObterPorContaFilhoteAsync(contaId, ct)
            ?? throw new InvalidOperationException($"Custódia não encontrada para conta {contaId}.");

        // Carteira atual
        decimal valorTotalInvestido = 0;
        decimal valorAtualCarteira = 0;

        foreach (var item in custodia.Itens)
        {
            decimal precoAtual;
            try
            {
                precoAtual = await _cotacoes.ObterCotacaoFechamentoAsync(item.Ticker, ct);
            }
            catch
            {
                precoAtual = item.PrecoMedio;
            }

            valorTotalInvestido += item.Quantidade * item.PrecoMedio;
            valorAtualCarteira += item.Quantidade * precoAtual;
        }

        var rentabTotal = valorTotalInvestido > 0
            ? ((valorAtualCarteira - valorTotalInvestido) / valorTotalInvestido) * 100m
            : 0m;

        // Histórico de aportes (distribuições)
        var distribuicoes = await _distribuicoes.ObterPorClienteAsync(clienteId, ct);
        var distribuicoesOrdenadas = distribuicoes.OrderBy(d => d.DataDistribuicao).ToList();

        var historicoAportes = distribuicoesOrdenadas
            .Select(d => new HistoricoAporteItem(
                d.DataDistribuicao,
                Math.Round(d.ValorAporte, 2),
                MapearParcela(d.DataDistribuicao.Day)))
            .ToList();

        // Evolução da carteira: valor investido acumulado em cada data de compra
        decimal investidoAcumulado = 0;
        var evolucao = new List<EvolucaoCarteiraItem>();

        foreach (var dist in distribuicoesOrdenadas)
        {
            investidoAcumulado += dist.ValorAporte;

            evolucao.Add(new EvolucaoCarteiraItem(
                dist.DataDistribuicao,
                Math.Round(investidoAcumulado, 2),
                Math.Round(valorAtualCarteira, 2)));
        }

        return new RentabilidadeResponse(
            cliente.Id,
            cliente.Nome,
            Math.Round(valorTotalInvestido, 2),
            Math.Round(valorAtualCarteira, 2),
            Math.Round(rentabTotal, 2),
            historicoAportes,
            evolucao);
    }

    private static string MapearParcela(int dia) => dia switch
    {
        5 => "1/3",
        15 => "2/3",
        25 => "3/3",
        _ => "1/1"
    };
}
