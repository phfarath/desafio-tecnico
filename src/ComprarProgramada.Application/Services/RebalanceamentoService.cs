using ComprarProgramada.Application.DTOs.Rebalanceamento;
using ComprarProgramada.Application.Events;
using ComprarProgramada.Domain.Interfaces;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.Interfaces.Services;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace ComprarProgramada.Application.Services;

public sealed class RebalanceamentoService : IRebalanceamentoService
{
    private readonly IClienteRepository _clientes;
    private readonly ICestaTopFiveRepository _cestas;
    private readonly ICustodiaFilhoteRepository _custodiasFilhote;
    private readonly ICotacaoService _cotacoes;
    private readonly IEventPublisher _eventos;
    private readonly IUnitOfWork _uow;
    private readonly string _topicoIrVenda;

    public RebalanceamentoService(
        IClienteRepository clientes,
        ICestaTopFiveRepository cestas,
        ICustodiaFilhoteRepository custodiasFilhote,
        ICotacaoService cotacoes,
        IEventPublisher eventos,
        IConfiguration configuration,
        IUnitOfWork uow)
    {
        _clientes = clientes;
        _cestas = cestas;
        _custodiasFilhote = custodiasFilhote;
        _cotacoes = cotacoes;
        _eventos = eventos;
        _uow = uow;
        _topicoIrVenda = configuration["Kafka:TopicoIrVenda"] ?? "ir-venda-rebalanceamento";
    }

    public async Task<RebalanceamentoResponse> ExecutarAsync(
        int cestaAnteriorId,
        int novaCestaId,
        CancellationToken ct = default)
    {
        var cestaAnterior = await _cestas.ObterPorIdAsync(cestaAnteriorId, ct)
            ?? throw new InvalidOperationException($"Cesta anterior {cestaAnteriorId} não encontrada.");

        var novaCesta = await _cestas.ObterPorIdAsync(novaCestaId, ct)
            ?? throw new InvalidOperationException($"Nova cesta {novaCestaId} não encontrada.");

        // Determina o que mudou
        var tickersAnteriores = cestaAnterior.Tickers.ToHashSet();
        var tickersNovos = novaCesta.Tickers.ToHashSet();

        var tickersSaindo = tickersAnteriores.Except(tickersNovos).ToList();
        var tickersEntrando = tickersNovos.Except(tickersAnteriores).ToList();

        if (tickersSaindo.Count == 0 && tickersEntrando.Count == 0)
            return new RebalanceamentoResponse(0, 0, 0, 0, []);

        // Cotações de todos os tickers envolvidos
        var todosTickers = tickersAnteriores.Union(tickersNovos).ToList();
        var cotacoesMap = await _cotacoes.ObterCotacoesAsync(todosTickers, ct);

        // Pesos relativos dos tickers novos dentro do que está entrando
        var pesoNovosTotal = novaCesta.Itens
            .Where(i => tickersEntrando.Contains(i.Ticker))
            .Sum(i => i.FracaoDecimal);

        var clientes = await _clientes.ObterAtivosAsync(ct);
        var detalhes = new List<RebalanceamentoClienteResponse>();

        foreach (var cliente in clientes)
        {
            var custodia = await _custodiasFilhote.ObterPorContaFilhoteAsync(
                cliente.ContaFilhote!.Id, ct);

            if (custodia is null) continue;

            var vendasDetalhe = new List<DetalheVendaItem>();
            decimal totalVendasCliente = 0;
            decimal lucroLiquidoCliente = 0;

            // 1. Vende tickers que saíram da cesta
            foreach (var ticker in tickersSaindo)
            {
                var item = custodia.ObterItem(ticker);
                if (item is null || item.Quantidade == 0) continue;

                cotacoesMap.TryGetValue(ticker.Valor, out var precoVenda);
                if (precoVenda == 0) precoVenda = item.PrecoMedio; // fallback: usa PM

                var qtd = item.Quantidade;
                var valorVenda = qtd * precoVenda;
                var lucro = valorVenda - qtd * item.PrecoMedio;

                vendasDetalhe.Add(new DetalheVendaItem(
                    ticker.Valor, qtd, precoVenda, item.PrecoMedio, lucro));

                totalVendasCliente += valorVenda;
                lucroLiquidoCliente += lucro;

                custodia.RemoverAtivos(ticker, qtd);
            }

            // 2. Compra tickers novos com o produto das vendas
            if (totalVendasCliente > 0 && tickersEntrando.Count > 0 && pesoNovosTotal > 0)
            {
                foreach (var cestaItem in novaCesta.Itens
                    .Where(i => tickersEntrando.Contains(i.Ticker)))
                {
                    var proporcao = cestaItem.FracaoDecimal / pesoNovosTotal;
                    var valorDisponivel = totalVendasCliente * proporcao;

                    cotacoesMap.TryGetValue(cestaItem.Ticker.Valor, out var precoCompra);
                    if (precoCompra <= 0) continue;

                    var qtdComprar = (int)(valorDisponivel / precoCompra); // TRUNCAR
                    if (qtdComprar <= 0) continue;

                    custodia.AdicionarAtivos(cestaItem.Ticker, qtdComprar, precoCompra);
                }
            }

            // 3. IR sobre vendas > R$20.000
            bool irPublicado = false;
            decimal valorIr = 0;

            if (totalVendasCliente > 20_000m && lucroLiquidoCliente > 0)
            {
                valorIr = lucroLiquidoCliente * 0.20m;

                var msg = IrVendaMessage.Criar(
                    clienteId: cliente.Id,
                    cpf: cliente.Cpf.Valor,
                    mesReferencia: DateTime.UtcNow.ToString("yyyy-MM"),
                    totalVendasMes: totalVendasCliente,
                    lucroLiquido: lucroLiquidoCliente,
                    valorIr: valorIr,
                    detalhes: vendasDetalhe);

                await _eventos.PublicarAsync(
                    _topicoIrVenda,
                    $"{cliente.Id}-{DateTime.UtcNow:yyyyMM}",
                    msg,
                    ct);

                irPublicado = true;
            }

            detalhes.Add(new RebalanceamentoClienteResponse(
                cliente.Id,
                cliente.Nome,
                totalVendasCliente,
                lucroLiquidoCliente,
                irPublicado,
                valorIr));
        }

        await _uow.CommitAsync(ct);

        return new RebalanceamentoResponse(
            TotalClientes: clientes.Count,
            TotalClientesComVendas: detalhes.Count(d => d.TotalVendas > 0),
            TotalVendasGeral: detalhes.Sum(d => d.TotalVendas),
            TotalIrPublicado: detalhes.Sum(d => d.ValorIr),
            Detalhes: detalhes);
    }
}
