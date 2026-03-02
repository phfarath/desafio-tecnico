using ComprarProgramada.Domain.Entities;

namespace ComprarProgramada.Domain.Interfaces.Repositories;

public interface IContaMasterRepository
{
    Task<ContaMaster?> ObterAsync(CancellationToken ct = default);
    Task<ContaMaster> ObterComCustodiaAsync(CancellationToken ct = default);
    Task AdicionarAsync(ContaMaster conta, CancellationToken ct = default);
    Task AtualizarAsync(ContaMaster conta, CancellationToken ct = default);
}
