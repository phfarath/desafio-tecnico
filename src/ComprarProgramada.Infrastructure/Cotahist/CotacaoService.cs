using ComprarProgramada.Domain.Interfaces.Services;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComprarProgramada.Infrastructure.Cotahist;

/// <summary>
/// Implementa <see cref="ICotacaoService"/> lendo o arquivo COTAHIST mais recente
/// da pasta configurada em "CotacoesPath" (padrão: "cotacoes/").
/// Mantém cache em memória (por execução) para evitar re-leitura do arquivo.
/// </summary>
public sealed class CotacaoService : ICotacaoService
{
    private readonly string _cotacoesPathConfig;
    private readonly ILogger<CotacaoService> _logger;

    // Cache: chave = "TICKER" (em maiúsculas), valor = preço de fechamento
    private Dictionary<string, decimal>? _cache;
    private string? _cachedFile;

    public CotacaoService(
        IConfiguration configuration, 
        ILogger<CotacaoService> logger)
    {
        _cotacoesPathConfig = configuration["CotacoesPath"] ?? "cotacoes";
        _logger = logger;
    }

    public Task<decimal> ObterCotacaoFechamentoAsync(Ticker ticker, CancellationToken ct = default)
    {
        var cache = ObterOuConstruirCache();

        if (!cache.TryGetValue(ticker.Valor, out var preco))
            throw new InvalidOperationException(
                $"Cotação não encontrada para o ticker '{ticker.Valor}' no arquivo '{_cachedFile}'.");

        return Task.FromResult(preco);
    }

    public Task<IDictionary<string, decimal>> ObterCotacoesAsync(
        IEnumerable<Ticker> tickers,
        CancellationToken ct = default)
    {
        var cache = ObterOuConstruirCache();
        var resultado = new Dictionary<string, decimal>();
        var naoEncontrados = new List<string>();

        foreach (var ticker in tickers)
        {
            if (cache.TryGetValue(ticker.Valor, out var preco))
                resultado[ticker.Valor] = preco;
            else
                naoEncontrados.Add(ticker.Valor);
        }

        if (naoEncontrados.Count > 0)
            throw new InvalidOperationException(
                $"Cotações não encontradas para: {string.Join(", ", naoEncontrados)} " +
                $"no arquivo '{_cachedFile}'.");

        return Task.FromResult<IDictionary<string, decimal>>(resultado);
    }

    // -------------------------------------------------------------------------

    private Dictionary<string, decimal> ObterOuConstruirCache()
    {
        var arquivo = ObterArquivoMaisRecente();

        // Reusa o cache se o arquivo não mudou
        if (_cache is not null && _cachedFile == arquivo)
            return _cache;

        _logger.LogInformation("Carregando cotações de {Arquivo}", arquivo);

        _cache = CotahistParser.Parse(arquivo)
            .Where(c => c.TipoMercado == 10) // apenas mercado à vista para preço de referência
            .GroupBy(c => c.Ticker)
            .ToDictionary(g => g.Key, g => g.First().PrecoFechamento);

        _cachedFile = arquivo;

        _logger.LogInformation("Cache carregado: {Count} tickers", _cache.Count);

        return _cache;
    }

    private string ObterArquivoMaisRecente()
    {
        var pastaCotacoes = ResolverPastaCotacoes()
            ?? throw new DirectoryNotFoundException(
                $"Pasta de cotacoes nao encontrada para 'CotacoesPath={_cotacoesPathConfig}'. " +
                $"Caminhos tentados: {string.Join(", ", ObterCaminhosCandidatos())}. " +
                "Configure 'CotacoesPath' no appsettings.json e adicione o arquivo COTAHIST_D*.TXT.");

        var arquivo = Directory
            .GetFiles(pastaCotacoes, "COTAHIST_D*.TXT")
            .OrderByDescending(f => f)
            .FirstOrDefault()
            ?? throw new FileNotFoundException(
                $"Nenhum arquivo COTAHIST_D*.TXT encontrado em '{pastaCotacoes}'.");

        return arquivo;
    }

    private string? ResolverPastaCotacoes()
    {
        foreach (var caminho in ObterCaminhosCandidatos())
        {
            if (Directory.Exists(caminho))
                return caminho;
        }

        return null;
    }

    private IEnumerable<string> ObterCaminhosCandidatos()
    {
        if (Path.IsPathRooted(_cotacoesPathConfig))
        {
            yield return Path.GetFullPath(_cotacoesPathConfig);
            yield break;
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        yield return Path.GetFullPath(Path.Combine(currentDirectory, _cotacoesPathConfig));
        yield return Path.GetFullPath(Path.Combine(currentDirectory, "..", _cotacoesPathConfig));
        yield return Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", _cotacoesPathConfig));

        var baseDirectory = AppContext.BaseDirectory;
        yield return Path.GetFullPath(Path.Combine(baseDirectory, _cotacoesPathConfig));
        yield return Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", _cotacoesPathConfig));
        yield return Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", _cotacoesPathConfig));
        yield return Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", _cotacoesPathConfig));
    }
}
