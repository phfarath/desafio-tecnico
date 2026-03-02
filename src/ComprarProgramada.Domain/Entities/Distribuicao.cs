using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Registro da alocação de ativos da conta master para a conta filhote de um cliente.
/// Gerado para cada cliente a cada data de compra.
/// </summary>
public class Distribuicao : Entity
{
    public int OrdemCompraId { get; private set; }
    public int ClienteId { get; private set; }
    public int ContaFilhoteId { get; private set; }
    public DateOnly DataDistribuicao { get; private set; }

    /// <summary>Valor do aporte do cliente nesta data (ValorMensal / 3).</summary>
    public decimal ValorAporte { get; private set; }

    /// <summary>Soma do IR dedo-duro de todos os itens desta distribuição.</summary>
    public decimal ValorTotalIrDedoDuro { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private readonly List<DistribuicaoItem> _itens = [];
    public IReadOnlyCollection<DistribuicaoItem> Itens => _itens.AsReadOnly();

    // Navigation
    public OrdemCompra? OrdemCompra { get; private set; }
    public Cliente? Cliente { get; private set; }

    protected Distribuicao() { }

    public static Distribuicao Criar(
        int ordemCompraId,
        int clienteId,
        int contaFilhoteId,
        DateOnly dataDistribuicao,
        decimal valorAporte)
    {
        if (valorAporte <= 0)
            throw new DomainException("O valor do aporte deve ser maior que zero.");

        return new Distribuicao
        {
            OrdemCompraId = ordemCompraId,
            ClienteId = clienteId,
            ContaFilhoteId = contaFilhoteId,
            DataDistribuicao = dataDistribuicao,
            ValorAporte = valorAporte,
            CriadoEm = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Registra a distribuição de um ativo e acumula o IR dedo-duro.
    /// </summary>
    public void AdicionarItem(Ticker ticker, int quantidade, decimal precoUnitario)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (precoUnitario <= 0)
            throw new DomainException("Preço unitário deve ser maior que zero.");

        var item = DistribuicaoItem.Criar(ticker, quantidade, precoUnitario);
        _itens.Add(item);
        ValorTotalIrDedoDuro += item.ValorIrDedoDuro;
    }

    public decimal ValorTotalDistribuido => _itens.Sum(i => i.ValorOperacao);
}
