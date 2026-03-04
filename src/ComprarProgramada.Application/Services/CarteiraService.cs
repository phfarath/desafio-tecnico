using ComprarProgramada.Application.DTOs.Carteira;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.Interfaces.Services;

namespace ComprarProgramada.Application.Services;

public sealed class CarteiraService : ICarteiraService
{
    private readonly IClienteRepository _clientes;
    private readonly ICustodiaFilhoteRepository _custodias;
    private readonly ICotacaoService _cotacoes;

    public CarteiraService(
        IClienteRepository clientes,
        ICustodiaFilhoteRepository custodias,
        ICotacaoService cotacoes)
    {
        _clientes = clientes;
        _custodias = custodias;
        _cotacoes = cotacoes;
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
}
