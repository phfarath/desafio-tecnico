using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Custódia de ativos residuais da conta master.
/// Residuos são ativos que sobram após a distribuição proporcional
/// e são reutilizados na próxima data de compra.
/// </summary>
public class CustodiaMaster : Entity
{
    public int ContaMasterId { get; private set; }

    private readonly List<CustodiaMasterItem> _itens = [];
    public IReadOnlyCollection<CustodiaMasterItem> Itens => _itens.AsReadOnly();

    // Navigation
    public ContaMaster? ContaMaster { get; private set; }

    protected CustodiaMaster() { }

    public static CustodiaMaster Criar(int contaMasterId)
    {
        return new CustodiaMaster { ContaMasterId = contaMasterId };
    }

    public void AdicionarAtivos(Ticker ticker, int quantidade, decimal precoUnitario)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        var item = _itens.FirstOrDefault(i => i.Ticker == ticker);

        if (item is null)
        {
            var novo = CustodiaMasterItem.Criar(ticker, quantidade, precoUnitario);
            _itens.Add(novo);
        }
        else
        {
            item.AdicionarAcoes(quantidade, precoUnitario);
        }
    }

    public void RemoverAtivos(Ticker ticker, int quantidade)
    {
        var item = _itens.FirstOrDefault(i => i.Ticker == ticker)
            ?? throw new DomainException($"Ativo {ticker} não encontrado na custódia master.");

        item.RemoverAcoes(quantidade);

        if (item.Quantidade == 0)
            _itens.Remove(item);
    }

    public int ObterQuantidade(Ticker ticker) =>
        _itens.FirstOrDefault(i => i.Ticker == ticker)?.Quantidade ?? 0;

    public CustodiaMasterItem? ObterItem(Ticker ticker) =>
        _itens.FirstOrDefault(i => i.Ticker == ticker);
}
