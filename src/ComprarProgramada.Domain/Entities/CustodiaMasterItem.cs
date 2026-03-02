using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Posição residual de um ativo específico na conta master.
/// Residuos surgem após a distribuição proporcional (arredondamentos).
/// </summary>
public class CustodiaMasterItem : Entity
{
    public int CustodiaMasterId { get; private set; }
    public Ticker Ticker { get; private set; } = null!;
    public int Quantidade { get; private set; }
    public decimal PrecoMedio { get; private set; }

    protected CustodiaMasterItem() { }

    internal static CustodiaMasterItem Criar(Ticker ticker, int quantidade, decimal precoUnitario)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (precoUnitario <= 0)
            throw new DomainException("Preço unitário deve ser maior que zero.");

        return new CustodiaMasterItem
        {
            Ticker = ticker,
            Quantidade = quantidade,
            PrecoMedio = precoUnitario
        };
    }

    public void AdicionarAcoes(int quantidade, decimal precoNova)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        PrecoMedio = (Quantidade * PrecoMedio + quantidade * precoNova)
                     / (Quantidade + quantidade);
        Quantidade += quantidade;
    }

    public void RemoverAcoes(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (quantidade > Quantidade)
            throw new DomainException(
                $"Quantidade insuficiente na custódia master: possui {Quantidade}, tentativa de remover {quantidade}.");

        Quantidade -= quantidade;
    }
}
