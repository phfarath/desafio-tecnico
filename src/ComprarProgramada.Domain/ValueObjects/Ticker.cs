using System.Text.RegularExpressions;
using ComprarProgramada.Domain.Exceptions;

namespace ComprarProgramada.Domain.ValueObjects;

public sealed record Ticker
{
    // Ex: PETR4, VALE3, ITUB4F, WEGE3F
    private static readonly Regex TickerRegex =
        new(@"^[A-Z]{4}\d{1,2}F?$", RegexOptions.Compiled);

    public string Valor { get; }

    public bool EhFracionario => Valor.EndsWith('F');

    private Ticker(string valor) => Valor = valor;

    public static Ticker Criar(string valor)
    {
        var normalizado = (valor ?? string.Empty).Trim().ToUpperInvariant();
        if (!TickerRegex.IsMatch(normalizado))
            throw new DomainException($"Ticker inválido: '{valor}'.");
        return new Ticker(normalizado);
    }

    /// <summary>Retorna a versão fracionária do ticker (sufixo F).</summary>
    public Ticker ObterFracionario() =>
        EhFracionario ? this : new Ticker(Valor + "F");

    /// <summary>Retorna o ticker sem sufixo F (lote padrão).</summary>
    public Ticker ObterPadrao() =>
        EhFracionario ? new Ticker(Valor[..^1]) : this;

    public override string ToString() => Valor;
}
