using ComprarProgramada.Domain.Entities;

namespace ComprarProgramada.Domain.Interfaces.Repositories;

public interface IContaFilhoteRepository
{
    Task<ContaFilhote?> ObterPorClienteIdAsync(int clienteId, CancellationToken ct = default);
    Task<ContaFilhote?> ObterComCustodiaAsync(int clienteId, CancellationToken ct = default);
    Task<int> ObterProximoSequencialAsync(CancellationToken ct = default);
    Task AdicionarAsync(ContaFilhote conta, CancellationToken ct = default);
    Task AtualizarAsync(ContaFilhote conta, CancellationToken ct = default);
}
