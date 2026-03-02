using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Posição de um ativo específico na custódia de um cliente.
/// Mantém quantidade e preço médio ponderado de aquisição.
/// </summary>
public class CustodiaFilhoteItem : Entity
{
    public int CustodiaFilhoteId { get; private set; }
    public Ticker Ticker { get; private set; } = null!;
    public int Quantidade { get; private set; }
    public decimal PrecoMedio { get; private set; }

    protected CustodiaFilhoteItem() { }

    internal static CustodiaFilhoteItem Criar(Ticker ticker, int quantidade, decimal precoUnitario)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (precoUnitario <= 0)
            throw new DomainException("Preço unitário deve ser maior que zero.");

        return new CustodiaFilhoteItem
        {
            Ticker = ticker,
            Quantidade = quantidade,
            PrecoMedio = precoUnitario
        };
    }

    /// <summary>
    /// Acrescenta ações e recalcula o preço médio ponderado.
    /// PM = (QtdAnt × PMAnt + QtdNova × PrecoNova) / (QtdAnt + QtdNova)
    /// </summary>
    public void AdicionarAcoes(int quantidade, decimal precoNova)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (precoNova <= 0)
            throw new DomainException("Preço deve ser maior que zero.");

        PrecoMedio = (Quantidade * PrecoMedio + quantidade * precoNova)
                     / (Quantidade + quantidade);
        Quantidade += quantidade;
    }

    /// <summary>
    /// Remove ações da posição. O preço médio não é alterado em vendas.
    /// </summary>
    public void RemoverAcoes(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (quantidade > Quantidade)
            throw new DomainException(
                $"Quantidade insuficiente: possui {Quantidade}, tentativa de remover {quantidade}.");

        Quantidade -= quantidade;
    }

    public decimal ValorInvestido => Quantidade * PrecoMedio;
}
