using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Cesta de recomendação com exatamente 5 ações e percentuais que somam 100%.
/// Cada alteração da cesta desativa a anterior e cria uma nova, mantendo histórico.
/// </summary>
public class CestaTopFive : Entity
{
    public const int QuantidadeAtivos = 5;
    public const decimal SomaPercentuais = 100m;
    private const decimal ToleranciaPercentual = 0.001m;

    public string Nome { get; private set; } = string.Empty;
    public bool Ativa { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataDesativacao { get; private set; }

    private readonly List<CestaItem> _itens = [];
    public IReadOnlyCollection<CestaItem> Itens => _itens.AsReadOnly();

    protected CestaTopFive() { }

    /// <summary>
    /// Cria uma nova cesta validando: exatamente 5 ativos, soma = 100%, sem duplicatas.
    /// </summary>
    /// <param name="nome">Nome descritivo da cesta (ex: "Top Five - Fevereiro 2026").</param>
    /// <param name="composicao">Pares (ticker, percentual).</param>
    public static CestaTopFive Criar(
        string nome,
        IEnumerable<(string ticker, decimal percentual)> composicao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome da cesta é obrigatório.");

        var lista = composicao.ToList();

        if (lista.Count != QuantidadeAtivos)
            throw new DomainException(
                $"A cesta deve conter exatamente {QuantidadeAtivos} ativos. " +
                $"Quantidade informada: {lista.Count}.");

        var soma = lista.Sum(x => x.percentual);
        if (Math.Abs(soma - SomaPercentuais) > ToleranciaPercentual)
            throw new DomainException(
                $"A soma dos percentuais deve ser exatamente {SomaPercentuais}%. " +
                $"Soma atual: {soma}%.");

        var tickers = lista.Select(x => x.ticker.Trim().ToUpperInvariant()).ToList();
        if (tickers.Distinct().Count() != tickers.Count)
            throw new DomainException("A cesta não pode conter ativos duplicados.");

        var cesta = new CestaTopFive
        {
            Nome = nome.Trim(),
            Ativa = true,
            DataCriacao = DateTime.UtcNow
        };

        foreach (var (ticker, percentual) in lista)
            cesta._itens.Add(CestaItem.Criar(ticker, percentual));

        return cesta;
    }

    public void Desativar()
    {
        if (!Ativa)
            throw new DomainException("A cesta já está inativa.");

        Ativa = false;
        DataDesativacao = DateTime.UtcNow;
    }

    public CestaItem ObterItem(Ticker ticker) =>
        _itens.FirstOrDefault(i => i.Ticker == ticker)
        ?? throw new DomainException($"Ativo {ticker} não encontrado na cesta.");

    public IEnumerable<Ticker> Tickers =>
        _itens.Select(i => i.Ticker);
}
