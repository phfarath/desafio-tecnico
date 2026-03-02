using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Item de uma ordem de compra para um ativo específico.
/// Registra a separação entre lote padrão e mercado fracionário.
/// </summary>
public class OrdemCompraItem : Entity
{
    public int OrdemCompraId { get; private set; }
    public Ticker Ticker { get; private set; } = null!;

    /// <summary>Quantidade comprada em lote padrão (múltiplos de 100).</summary>
    public int QuantidadeLotePadrao { get; private set; }

    /// <summary>Quantidade comprada no mercado fracionário (1-99).</summary>
    public int QuantidadeFracionario { get; private set; }

    public decimal PrecoUnitario { get; private set; }

    protected OrdemCompraItem() { }

    internal static OrdemCompraItem Criar(
        Ticker ticker,
        int qtdTotal,
        decimal precoUnitario)
    {
        // RN-031/032: lote padrão = multiplos de 100, fracionário = restante
        var lotePadrao = (qtdTotal / 100) * 100;
        var fracionario = qtdTotal % 100;

        return new OrdemCompraItem
        {
            Ticker = ticker,
            QuantidadeLotePadrao = lotePadrao,
            QuantidadeFracionario = fracionario,
            PrecoUnitario = precoUnitario
        };
    }

    public int QuantidadeTotal => QuantidadeLotePadrao + QuantidadeFracionario;
    public decimal ValorTotal => QuantidadeTotal * PrecoUnitario;
    public decimal ValorLotePadrao => QuantidadeLotePadrao * PrecoUnitario;
    public decimal ValorFracionario => QuantidadeFracionario * PrecoUnitario;

    /// <summary>Ticker fracionário (com sufixo F), usado apenas se houver qty fracionária.</summary>
    public Ticker TickerFracionario => Ticker.ObterFracionario();
}
