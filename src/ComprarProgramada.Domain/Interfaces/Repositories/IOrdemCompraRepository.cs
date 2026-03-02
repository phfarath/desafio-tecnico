using ComprarProgramada.Domain.Entities;

namespace ComprarProgramada.Domain.Interfaces.Repositories;

public interface IOrdemCompraRepository
{
    Task<OrdemCompra?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<OrdemCompra?> ObterPorDataAsync(DateOnly data, CancellationToken ct = default);
    Task<bool> ExisteParaDataAsync(DateOnly data, CancellationToken ct = default);
    Task AdicionarAsync(OrdemCompra ordem, CancellationToken ct = default);
    Task AtualizarAsync(OrdemCompra ordem, CancellationToken ct = default);
}
