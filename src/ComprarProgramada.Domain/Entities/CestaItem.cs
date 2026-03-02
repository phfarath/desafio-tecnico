using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Um dos 5 ativos que compõem a Cesta Top Five, com seu percentual de alocação.
/// </summary>
public class CestaItem : Entity
{
    public int CestaTopFiveId { get; private set; }
    public Ticker Ticker { get; private set; } = null!;
    public decimal Percentual { get; private set; }

    protected CestaItem() { }

    internal static CestaItem Criar(string ticker, decimal percentual)
    {
        if (percentual <= 0)
            throw new DomainException(
                $"O percentual do ativo '{ticker}' deve ser maior que 0%.");

        return new CestaItem
        {
            Ticker = Ticker.Criar(ticker),
            Percentual = percentual
        };
    }

    /// <summary>Percentual como fração decimal (ex: 30% → 0,30).</summary>
    public decimal FracaoDecimal => Percentual / 100m;
}
