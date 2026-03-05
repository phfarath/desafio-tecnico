using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Custódia de ativos de um cliente. Aggregate root que concentra
/// as posições (CustodiaFilhoteItem) por ticker.
/// </summary>
public class CustodiaFilhote : Entity
{
    public int ContaFilhoteId { get; private set; }

    private readonly List<CustodiaFilhoteItem> _itens = [];
    public IReadOnlyCollection<CustodiaFilhoteItem> Itens => _itens.AsReadOnly();

    // Navigation
    public ContaFilhote? ContaFilhote { get; private set; }

    protected CustodiaFilhote() { }

    public static CustodiaFilhote Criar(ContaFilhote contaFilhote)
    {
        ArgumentNullException.ThrowIfNull(contaFilhote);

        return new CustodiaFilhote { ContaFilhote = contaFilhote };
    }

    public static CustodiaFilhote Criar(int contaFilhoteId)
    {
        return new CustodiaFilhote { ContaFilhoteId = contaFilhoteId };
    }

    /// <summary>
    /// Adiciona ativos à custódia. Se o ticker já existir, recalcula o preço médio.
    /// </summary>
    public void AdicionarAtivos(Ticker ticker, int quantidade, decimal precoUnitario)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        var item = _itens.FirstOrDefault(i => i.Ticker == ticker);

        if (item is null)
        {
            var novo = CustodiaFilhoteItem.Criar(ticker, quantidade, precoUnitario);
            _itens.Add(novo);
        }
        else
        {
            item.AdicionarAcoes(quantidade, precoUnitario);
        }
    }

    /// <summary>
    /// Remove ativos da custódia. Se a quantidade chegar a zero, remove o item.
    /// </summary>
    public void RemoverAtivos(Ticker ticker, int quantidade)
    {
        var item = _itens.FirstOrDefault(i => i.Ticker == ticker)
            ?? throw new DomainException($"Ativo {ticker} não encontrado na custódia.");

        item.RemoverAcoes(quantidade);

        if (item.Quantidade == 0)
            _itens.Remove(item);
    }

    public CustodiaFilhoteItem? ObterItem(Ticker ticker) =>
        _itens.FirstOrDefault(i => i.Ticker == ticker);

    public bool PossuiAtivo(Ticker ticker) =>
        _itens.Any(i => i.Ticker == ticker && i.Quantidade > 0);
}
