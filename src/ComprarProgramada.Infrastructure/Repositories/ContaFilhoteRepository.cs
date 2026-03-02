using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprarProgramada.Infrastructure.Repositories;

public class ContaFilhoteRepository : IContaFilhoteRepository
{
    private readonly AppDbContext _context;

    public ContaFilhoteRepository(AppDbContext context) => _context = context;

    public async Task<ContaFilhote?> ObterPorClienteIdAsync(int clienteId, CancellationToken ct = default) =>
        await _context.ContasFilhote
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId, ct);

    public async Task<ContaFilhote?> ObterComCustodiaAsync(int clienteId, CancellationToken ct = default) =>
        await _context.ContasFilhote
            .Include(c => c.Custodia!)
                .ThenInclude(cf => cf.Itens)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId, ct);

    public async Task<int> ObterProximoSequencialAsync(CancellationToken ct = default) =>
        await _context.ContasFilhote.CountAsync(ct) + 1;

    public async Task AdicionarAsync(ContaFilhote conta, CancellationToken ct = default) =>
        await _context.ContasFilhote.AddAsync(conta, ct);

    public Task AtualizarAsync(ContaFilhote conta, CancellationToken ct = default)
    {
        _context.ContasFilhote.Update(conta);
        return Task.CompletedTask;
    }
}
