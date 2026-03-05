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

        // RN-049: tickers que permaneceram mas mudaram de percentual também precisam rebalancear
        var tickersPermanecendo = tickersAnteriores.Intersect(tickersNovos).ToList();
        var tickersMudandoPercentual = tickersPermanecendo
            .Where(t =>
            {
                var itemAnterior = cestaAnterior.Itens.FirstOrDefault(i => i.Ticker == t);
                var itemNovo = novaCesta.Itens.FirstOrDefault(i => i.Ticker == t);
                return itemAnterior is not null && itemNovo is not null &&
                       itemAnterior.FracaoDecimal != itemNovo.FracaoDecimal;
            })
            .ToList();

        bool haNadaParaFazer = tickersSaindo.Count == 0
                               && tickersEntrando.Count == 0
                               && tickersMudandoPercentual.Count == 0;

        if (haNadaParaFazer)
            return new RebalanceamentoResponse(0, 0, 0, 0, []);

        // Cotações de todos os tickers envolvidos
        var todosTickers = tickersAnteriores.Union(tickersNovos).ToList();
        var cotacoesMap = await _cotacoes.ObterCotacoesAsync(todosTickers, ct);

        // Pesos relativos dos tickers novos dentro do que está entrando
        var pesoNovosTotal = novaCesta.Itens
            .Where(i => tickersEntrando.Contains(i.Ticker))
            .Sum(i => i.FracaoDecimal);

        var clientes = await _clientes.ObterAtivosAsync(ct);
        var clientesSemConta = clientes
            .Where(c => c.ContaFilhote is null)
            .Select(c => c.Id)
            .ToList();

        if (clientesSemConta.Count > 0)
            throw new InvalidOperationException(
                $"Clientes ativos sem conta filhote associada: {string.Join(", ", clientesSemConta)}.");

        var detalhes = new List<RebalanceamentoClienteResponse>();

        foreach (var cliente in clientes)
        {
            if (cliente.ContaFilhote is null) continue;

            var custodia = await _custodiasFilhote.ObterPorContaFilhoteAsync(
                cliente.ContaFilhote.Id, ct);

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

            // 2. RN-049: Rebalanceia tickers que permaneceram mas mudaram de percentual
            decimal totalComprasRebalanceamento = 0;

            if (tickersMudandoPercentual.Count > 0)
            {
                // Calcula o valor total da carteira atual (todos os tickers novos que o cliente tem)
                decimal valorCarteiraTotal = 0;
                foreach (var ticker in tickersNovos)
                {
                    var item = custodia.ObterItem(ticker);
                    if (item is null || item.Quantidade == 0) continue;

                    cotacoesMap.TryGetValue(ticker.Valor, out var preco);
                    if (preco == 0) preco = item.PrecoMedio;
                    valorCarteiraTotal += item.Quantidade * preco;
                }

                foreach (var ticker in tickersMudandoPercentual)
                {
                    var item = custodia.ObterItem(ticker);
                    if (item is null || item.Quantidade == 0) continue;

                    cotacoesMap.TryGetValue(ticker.Valor, out var preco);
                    if (preco == 0) preco = item.PrecoMedio;
                    if (preco <= 0) continue;

                    var novoItem = novaCesta.Itens.First(i => i.Ticker == ticker);
                    var valorAlvo = valorCarteiraTotal * novoItem.FracaoDecimal;
                    var valorAtual = item.Quantidade * preco;
                    var diferenca = valorAtual - valorAlvo;

                    if (diferenca > preco) // excesso: vender
                    {
                        var qtdVender = (int)(diferenca / preco);
                        if (qtdVender <= 0 || qtdVender > item.Quantidade) continue;

                        var valorVenda = qtdVender * preco;
                        var lucro = valorVenda - qtdVender * item.PrecoMedio;

                        vendasDetalhe.Add(new DetalheVendaItem(
                            ticker.Valor, qtdVender, preco, item.PrecoMedio, lucro));

                        totalVendasCliente += valorVenda;
                        lucroLiquidoCliente += lucro;

                        custodia.RemoverAtivos(ticker, qtdVender);
                    }
                    else if (diferenca < -preco) // déficit: comprar
                    {
                        var qtdComprar = (int)((-diferenca) / preco);
                        if (qtdComprar <= 0) continue;

                        custodia.AdicionarAtivos(ticker, qtdComprar, preco);
                        totalComprasRebalanceamento += qtdComprar * preco;
                    }
                }
            }

            // 3. Compra tickers novos com o produto das vendas
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

            // 4. IR sobre vendas > R$20.000
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

    /// <summary>
    /// RN-050: Rebalanceia carteiras cujo desvio de proporção em relação à cesta ativa
    /// supera o limiar informado (em pontos percentuais).
    /// </summary>
    public async Task<RebalanceamentoResponse> ExecutarPorDesvioAsync(
        decimal limiarDesvioPercentual = 5m,
        CancellationToken ct = default)
    {
        var cestaAtiva = await _cestas.ObterAtivaAsync(ct)
            ?? throw new InvalidOperationException("Nenhuma cesta Top Five ativa encontrada.");

        var tickers = cestaAtiva.Tickers.ToList();
        var cotacoesMap = await _cotacoes.ObterCotacoesAsync(tickers, ct);

        var clientes = await _clientes.ObterAtivosAsync(ct);
        var detalhes = new List<RebalanceamentoClienteResponse>();

        foreach (var cliente in clientes)
        {
            if (cliente.ContaFilhote is null) continue;

            var custodia = await _custodiasFilhote.ObterPorContaFilhoteAsync(
                cliente.ContaFilhote.Id, ct);

            if (custodia is null) continue;

            // Calcula valor total da carteira
            decimal valorCarteiraTotal = 0;
            foreach (var ticker in tickers)
            {
                var item = custodia.ObterItem(ticker);
                if (item is null || item.Quantidade == 0) continue;

                cotacoesMap.TryGetValue(ticker.Valor, out var preco);
                if (preco == 0) preco = item.PrecoMedio;
                valorCarteiraTotal += item.Quantidade * preco;
            }

            if (valorCarteiraTotal == 0) continue;

            // Verifica se algum ticker tem desvio acima do limiar
            bool temDesvio = cestaAtiva.Itens.Any(cestaItem =>
            {
                var item = custodia.ObterItem(cestaItem.Ticker);
                cotacoesMap.TryGetValue(cestaItem.Ticker.Valor, out var preco);
                if (preco == 0) preco = item?.PrecoMedio ?? 0;

                var qtd = item?.Quantidade ?? 0;
                var valorAtual = qtd * preco;
                var pesoAtual = valorCarteiraTotal > 0 ? (valorAtual / valorCarteiraTotal) * 100m : 0m;
                var pesoAlvo = cestaItem.FracaoDecimal * 100m;

                return Math.Abs(pesoAtual - pesoAlvo) >= limiarDesvioPercentual;
            });

            if (!temDesvio) continue;

            var vendasDetalhe = new List<DetalheVendaItem>();
            decimal totalVendasCliente = 0;
            decimal lucroLiquidoCliente = 0;

            // Rebalanceia cada ticker com desvio
            foreach (var cestaItem in cestaAtiva.Itens)
            {
                var item = custodia.ObterItem(cestaItem.Ticker);
                cotacoesMap.TryGetValue(cestaItem.Ticker.Valor, out var preco);
                if (preco == 0) preco = item?.PrecoMedio ?? 0;
                if (preco <= 0) continue;

                var valorAlvo = valorCarteiraTotal * cestaItem.FracaoDecimal;
                var qtdAtual = item?.Quantidade ?? 0;
                var valorAtual = qtdAtual * preco;
                var diferenca = valorAtual - valorAlvo;

                if (diferenca > preco && qtdAtual > 0) // excesso: vender
                {
                    var qtdVender = (int)(diferenca / preco);
                    if (qtdVender <= 0 || qtdVender > qtdAtual) continue;

                    var valorVenda = qtdVender * preco;
                    var lucro = valorVenda - qtdVender * item!.PrecoMedio;

                    vendasDetalhe.Add(new DetalheVendaItem(
                        cestaItem.Ticker.Valor, qtdVender, preco, item.PrecoMedio, lucro));

                    totalVendasCliente += valorVenda;
                    lucroLiquidoCliente += lucro;

                    custodia.RemoverAtivos(cestaItem.Ticker, qtdVender);
                }
                else if (diferenca < -preco) // déficit: comprar
                {
                    var qtdComprar = (int)((-diferenca) / preco);
                    if (qtdComprar <= 0) continue;

                    custodia.AdicionarAtivos(cestaItem.Ticker, qtdComprar, preco);
                }
            }

            // IR sobre vendas > R$20.000
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
