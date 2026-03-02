using ComprarProgramada.Domain.Entities;

namespace ComprarProgramada.Domain.Interfaces.Repositories;

public interface ICustodiaFilhoteRepository
{
    Task<CustodiaFilhote?> ObterPorContaFilhoteAsync(int contaFilhoteId, CancellationToken ct = default);
    Task AdicionarAsync(CustodiaFilhote custodia, CancellationToken ct = default);
    Task AtualizarAsync(CustodiaFilhote custodia, CancellationToken ct = default);
}
