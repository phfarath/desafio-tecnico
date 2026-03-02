namespace ComprarProgramada.Infrastructure.Cotahist;

public sealed class CotacaoB3
{
    public DateOnly DataPregao { get; init; }
    public string Ticker { get; init; } = string.Empty;
    public string CodigoBdi { get; init; } = string.Empty;
    public int TipoMercado { get; init; }
    public decimal PrecoAbertura { get; init; }
    public decimal PrecoMaximo { get; init; }
    public decimal PrecoMinimo { get; init; }
    public decimal PrecoFechamento { get; init; }
}
