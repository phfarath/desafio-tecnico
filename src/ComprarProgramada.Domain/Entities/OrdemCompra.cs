using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Enums;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Registro de uma compra consolidada executada na conta master.
/// Uma ordem por data de compra (dias 5/15/25).
/// </summary>
public class OrdemCompra : Entity
{
    public int ContaMasterId { get; private set; }
    public int CestaTopFiveId { get; private set; }
    public DateOnly DataCompra { get; private set; }
    public decimal ValorTotalConsolidado { get; private set; }
    public StatusOrdem Status { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private readonly List<OrdemCompraItem> _itens = [];
    public IReadOnlyCollection<OrdemCompraItem> Itens => _itens.AsReadOnly();

    // Navigation
    public ContaMaster? ContaMaster { get; private set; }
    public CestaTopFive? CestaTopFive { get; private set; }

    protected OrdemCompra() { }

    public static OrdemCompra Criar(
        int contaMasterId,
        int cestaTopFiveId,
        DateOnly dataCompra,
        decimal valorTotalConsolidado)
    {
        if (valorTotalConsolidado <= 0)
            throw new DomainException("O valor total consolidado deve ser maior que zero.");

        return new OrdemCompra
        {
            ContaMasterId = contaMasterId,
            CestaTopFiveId = cestaTopFiveId,
            DataCompra = dataCompra,
            ValorTotalConsolidado = valorTotalConsolidado,
            Status = StatusOrdem.Pendente,
            CriadoEm = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adiciona um ativo à ordem. Calcula automaticamente a divisão lote/fracionário.
    /// </summary>
    public void AdicionarItem(Ticker ticker, int quantidade, decimal precoUnitario)
    {
        if (Status != StatusOrdem.Pendente)
            throw new DomainException("Não é possível adicionar itens a uma ordem já processada.");
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");
        if (precoUnitario <= 0)
            throw new DomainException("Preço unitário deve ser maior que zero.");
        if (_itens.Any(i => i.Ticker == ticker))
            throw new DomainException($"Ativo {ticker} já adicionado à ordem.");

        _itens.Add(OrdemCompraItem.Criar(ticker, quantidade, precoUnitario));
    }

    public void Executar()
    {
        if (Status != StatusOrdem.Pendente)
            throw new DomainException("A ordem já foi processada.");
        if (!_itens.Any())
            throw new DomainException("A ordem não possui itens.");

        Status = StatusOrdem.Executada;
    }

    public void Cancelar()
    {
        if (Status == StatusOrdem.Executada)
            throw new DomainException("Não é possível cancelar uma ordem já executada.");

        Status = StatusOrdem.Cancelada;
    }

    public decimal ValorTotalExecutado => _itens.Sum(i => i.ValorTotal);
}
