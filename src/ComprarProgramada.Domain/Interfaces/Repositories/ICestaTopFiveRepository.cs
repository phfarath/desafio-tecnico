using ComprarProgramada.Domain.Entities;

namespace ComprarProgramada.Domain.Interfaces.Repositories;

public interface ICestaTopFiveRepository
{
    Task<CestaTopFive?> ObterAtivaAsync(CancellationToken ct = default);
    Task<CestaTopFive?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CestaTopFive>> ObterHistoricoAsync(CancellationToken ct = default);
    Task AdicionarAsync(CestaTopFive cesta, CancellationToken ct = default);
    Task AtualizarAsync(CestaTopFive cesta, CancellationToken ct = default);
}
