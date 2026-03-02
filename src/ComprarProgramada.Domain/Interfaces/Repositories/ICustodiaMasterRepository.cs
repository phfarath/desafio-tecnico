using ComprarProgramada.Domain.Entities;

namespace ComprarProgramada.Domain.Interfaces.Repositories;

public interface ICustodiaMasterRepository
{
    Task<CustodiaMaster?> ObterAsync(CancellationToken ct = default);
    Task<CustodiaMaster> ObterComItensAsync(CancellationToken ct = default);
    Task AdicionarAsync(CustodiaMaster custodia, CancellationToken ct = default);
    Task AtualizarAsync(CustodiaMaster custodia, CancellationToken ct = default);
}
