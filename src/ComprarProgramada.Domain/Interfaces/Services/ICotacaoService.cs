using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Interfaces.Services;

/// <summary>
/// Fornece cotações de fechamento a partir dos arquivos COTAHIST da B3.
/// </summary>
public interface ICotacaoService
{
    /// <summary>
    /// Retorna a cotação de fechamento do último pregão disponível para o ticker informado.
    /// </summary>
    Task<decimal> ObterCotacaoFechamentoAsync(Ticker ticker, CancellationToken ct = default);

    /// <summary>
    /// Retorna as cotações de fechamento do último pregão para múltiplos tickers.
    /// </summary>
    Task<IDictionary<string, decimal>> ObterCotacoesAsync(
        IEnumerable<Ticker> tickers,
        CancellationToken ct = default);
}
