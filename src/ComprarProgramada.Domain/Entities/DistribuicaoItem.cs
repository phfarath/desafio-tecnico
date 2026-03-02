using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Registro da distribuição de um ativo específico para um cliente.
/// Contém o IR dedo-duro calculado (0,005% sobre o valor da operação).
/// </summary>
public class DistribuicaoItem : Entity
{
    public int DistribuicaoId { get; private set; }
    public Ticker Ticker { get; private set; } = null!;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal ValorOperacao { get; private set; }

    /// <summary>IR dedo-duro = 0,005% sobre o valor da operação (RN-053).</summary>
    public decimal ValorIrDedoDuro { get; private set; }

    protected DistribuicaoItem() { }

    internal static DistribuicaoItem Criar(
        Ticker ticker,
        int quantidade,
        decimal precoUnitario)
    {
        var valorOperacao = quantidade * precoUnitario;
        var valorIr = Math.Round(valorOperacao * 0.00005m, 2);

        return new DistribuicaoItem
        {
            Ticker = ticker,
            Quantidade = quantidade,
            PrecoUnitario = precoUnitario,
            ValorOperacao = valorOperacao,
            ValorIrDedoDuro = valorIr
        };
    }
}
