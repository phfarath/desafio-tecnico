using ComprarProgramada.Application.DTOs.Motor;
using ComprarProgramada.Application.Events;
using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.Interfaces.Services;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace ComprarProgramada.Application.Services;

public sealed class MotorCompraService : IMotorCompraService
{
    private readonly IClienteRepository _clientes;
    private readonly IContaMasterRepository _contaMaster;
    private readonly ICestaTopFiveRepository _cestas;
    private readonly IOrdemCompraRepository _ordens;
    private readonly IDistribuicaoRepository _distribuicoes;
    private readonly ICustodiaFilhoteRepository _custodiasFilhote;
    private readonly ICustodiaMasterRepository _custodiasMaster;
    private readonly ICotacaoService _cotacoes;
    private readonly IEventPublisher _eventos;
    private readonly string _topicoIrDedoDuro;
    private readonly IUnitOfWork _uow;

    public MotorCompraService(
        IClienteRepository clientes,
        IContaMasterRepository contaMaster,
        ICestaTopFiveRepository cestas,
        IOrdemCompraRepository ordens,
        IDistribuicaoRepository distribuicoes,
        ICustodiaFilhoteRepository custodiasFilhote,
        ICustodiaMasterRepository custodiasMaster,
        ICotacaoService cotacoes,
        IEventPublisher eventos,
        IConfiguration configuration,
        IUnitOfWork uow)
    {
        _clientes = clientes;
        _contaMaster = contaMaster;
        _cestas = cestas;
        _ordens = ordens;
        _distribuicoes = distribuicoes;
        _custodiasFilhote = custodiasFilhote;
        _custodiasMaster = custodiasMaster;
        _cotacoes = cotacoes;
        _eventos = eventos;
        _topicoIrDedoDuro = configuration["Kafka:TopicoIrDedoDuro"] ?? "ir-compra-programada";
        _uow = uow;
    }

    public async Task<ExecutarCompraResponse> ExecutarAsync(DateOnly dataCompra, CancellationToken ct = default)
    {
        // Idempotência: não executa duas vezes para a mesma data
        if (await _ordens.ExisteParaDataAsync(dataCompra, ct))
            throw new InvalidOperationException(
                $"Já existe uma ordem de compra para {dataCompra:dd/MM/yyyy}.");

        // 1. Carrega pré-requisitos
        var clientes = await _clientes.ObterAtivosAsync(ct);
        if (!clientes.Any())
            throw new InvalidOperationException("Nenhum cliente ativo para executar a compra.");

        var cesta = await _cestas.ObterAtivaAsync(ct)
            ?? throw new InvalidOperationException("Nenhuma cesta Top Five ativa.");

        var master = await _contaMaster.ObterAsync(ct)
            ?? throw new InvalidOperationException("Conta master não encontrada. Execute o seed inicial.");
        var custodiaMaster = await _custodiasMaster.ObterComItensAsync(ct);

        // 2. Consolida os aportes
        var totalConsolidado = clientes.Sum(c => c.ValorParcela);
        var tickers = cesta.Tickers.ToList();

        // 3. Obtém cotações de todos os tickers de uma vez
        var cotacoes = await _cotacoes.ObterCotacoesAsync(tickers, ct);

        // 4. Calcula quantidade a comprar por ativo (descontando saldo master)
        var compras = new Dictionary<string, (int quantidade, decimal preco)>();
        foreach (var item in cesta.Itens)
        {
            var valorProporcional = totalConsolidado * item.FracaoDecimal;
            var preco = cotacoes[item.Ticker.Valor];
            var qtdTotal = (int)(valorProporcional / preco); // TRUNCAR

            // Desconta saldo na custódia master
            var saldoMaster = custodiaMaster.ObterItem(item.Ticker)?.Quantidade ?? 0;
            var qtdComprar = Math.Max(0, qtdTotal - saldoMaster);

            if (qtdComprar > 0)
                compras[item.Ticker.Valor] = (qtdComprar, preco);
        }

        // 5. Cria e persiste a ordem de compra
        var ordem = OrdemCompra.Criar(master.Id, cesta.Id, dataCompra, totalConsolidado);
        foreach (var (tickerValor, (qtd, preco)) in compras)
            ordem.AdicionarItem(Ticker.Criar(tickerValor), qtd, preco);

        ordem.Executar();
        await _ordens.AdicionarAsync(ordem, ct);
        await _uow.CommitAsync(ct);

        // 6. Distribui para as custódias filhotes
        var distribuicoesCriadas = new List<Distribuicao>();
        foreach (var cliente in clientes)
        {
            var contaFilhoteId = cliente.ContaFilhote!.Id;
            var proporcao = cliente.ValorParcela / totalConsolidado;

            var dist = Distribuicao.Criar(ordem.Id, cliente.Id, contaFilhoteId, dataCompra, cliente.ValorParcela);

            foreach (var cestaItem in cesta.Itens)
            {
                var ticker = cestaItem.Ticker;
                var saldoMaster = custodiaMaster.ObterItem(ticker)?.Quantidade ?? 0;
                var qtdComprada = compras.TryGetValue(ticker.Valor, out var c) ? c.quantidade : 0;
                var qtdDisponivel = qtdComprada + saldoMaster;

                var qtdCliente = (int)(qtdDisponivel * proporcao); // TRUNCAR
                if (qtdCliente <= 0) continue;

                var preco = cotacoes[ticker.Valor];
                dist.AdicionarItem(ticker, qtdCliente, preco);
            }

            if (dist.Itens.Any())
                distribuicoesCriadas.Add(dist);
        }

        await _distribuicoes.AdicionarVariasAsync(distribuicoesCriadas, ct);

        // 7. Atualiza custódias filhotes
        foreach (var dist in distribuicoesCriadas)
        {
            var custodia = await _custodiasFilhote.ObterPorContaFilhoteAsync(dist.ContaFilhoteId, ct)
                ?? throw new InvalidOperationException(
                    $"Custódia filhote não encontrada para conta {dist.ContaFilhoteId}.");

            foreach (var item in dist.Itens)
                custodia.AdicionarAtivos(item.Ticker, item.Quantidade, item.PrecoUnitario);
        }

        // 8. Atualiza saldo residual na custódia master
        foreach (var cestaItem in cesta.Itens)
        {
            var ticker = cestaItem.Ticker;
            var saldoMaster = custodiaMaster.ObterItem(ticker)?.Quantidade ?? 0;
            var qtdComprada = compras.TryGetValue(ticker.Valor, out var c) ? c.quantidade : 0;
            var qtdDisponivel = qtdComprada + saldoMaster;
            var qtdDistribuida = distribuicoesCriadas.Sum(d =>
                d.Itens.FirstOrDefault(i => i.Ticker == ticker)?.Quantidade ?? 0);
            var residuo = qtdDisponivel - qtdDistribuida;

            // Zera saldo anterior e registra novo resíduo
            if (saldoMaster > 0)
                custodiaMaster.RemoverAtivos(ticker, saldoMaster);
            if (residuo > 0)
            {
                var preco = cotacoes[ticker.Valor];
                custodiaMaster.AdicionarAtivos(ticker, residuo, preco);
            }
        }

        await _uow.CommitAsync(ct);

        // 9. Publica eventos de IR dedo-duro para cada cliente/ativo
        await PublicarIrDedoDuroAsync(clientes, distribuicoesCriadas, ct);

        // 10. Monta resposta
        var itensComprados = ordem.Itens.Select(i => new ItemCompradoResponse(
            i.Ticker.Valor,
            i.QuantidadeLotePadrao,
            i.QuantidadeFracionario,
            i.QuantidadeLotePadrao + i.QuantidadeFracionario,
            i.PrecoUnitario,
            i.ValorTotal)).ToList();

        var residuosMaster = cesta.Itens
            .Select(ci => new
            {
                ticker = ci.Ticker.Valor,
                qtd = custodiaMaster.ObterItem(ci.Ticker)?.Quantidade ?? 0
            })
            .Where(r => r.qtd > 0)
            .Select(r => new ResidualMasterResponse(r.ticker, r.qtd))
            .ToList();

        return new ExecutarCompraResponse(
            ordem.Id,
            dataCompra,
            totalConsolidado,
            clientes.Count(),
            itensComprados,
            residuosMaster);
    }

    private async Task PublicarIrDedoDuroAsync(
        IEnumerable<Domain.Entities.Cliente> clientes,
        IEnumerable<Distribuicao> distribuicoes,
        CancellationToken ct)
    {
        var clienteMap = clientes.ToDictionary(c => c.Id);

        foreach (var dist in distribuicoes)
        {
            if (!clienteMap.TryGetValue(dist.ClienteId, out var cliente)) continue;

            foreach (var item in dist.Itens.Where(i => i.ValorIrDedoDuro > 0))
            {
                var msg = IrDedoDuroMessage.Criar(
                    clienteId: cliente.Id,
                    cpf: cliente.Cpf.Valor,
                    ticker: item.Ticker.Valor,
                    quantidade: item.Quantidade,
                    precoUnitario: item.PrecoUnitario,
                    valorOperacao: item.ValorOperacao,
                    valorIr: item.ValorIrDedoDuro);

                await _eventos.PublicarAsync(
                    _topicoIrDedoDuro,
                    $"{cliente.Id}-{item.Ticker.Valor}",
                    msg,
                    ct);
            }
        }
    }
}
