using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Interfaces.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Cliente?> ObterPorCpfAsync(Cpf cpf, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ObterAtivosAsync(CancellationToken ct = default);
    Task<bool> ExisteCpfAsync(Cpf cpf, CancellationToken ct = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken ct = default);
    Task AtualizarAsync(Cliente cliente, CancellationToken ct = default);
}
