using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprarProgramada.Infrastructure.Repositories;

public class CustodiaFilhoteRepository : ICustodiaFilhoteRepository
{
    private readonly AppDbContext _context;

    public CustodiaFilhoteRepository(AppDbContext context) => _context = context;

    public async Task<CustodiaFilhote?> ObterPorContaFilhoteAsync(int contaFilhoteId, CancellationToken ct = default) =>
        await _context.CustodiasFilhote
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.ContaFilhoteId == contaFilhoteId, ct);

    public async Task AdicionarAsync(CustodiaFilhote custodia, CancellationToken ct = default) =>
        await _context.CustodiasFilhote.AddAsync(custodia, ct);

    public Task AtualizarAsync(CustodiaFilhote custodia, CancellationToken ct = default)
    {
        _context.CustodiasFilhote.Update(custodia);
        return Task.CompletedTask;
    }
}
