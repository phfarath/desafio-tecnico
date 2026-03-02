using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprarProgramada.Infrastructure.Repositories;

public class CustodiaMasterRepository : ICustodiaMasterRepository
{
    private readonly AppDbContext _context;

    public CustodiaMasterRepository(AppDbContext context) => _context = context;

    public async Task<CustodiaMaster?> ObterAsync(CancellationToken ct = default) =>
        await _context.CustodiasMaster
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(ct);

    public async Task<CustodiaMaster> ObterComItensAsync(CancellationToken ct = default)
    {
        var custodia = await _context.CustodiasMaster
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(ct);

        return custodia ?? throw new DomainException("Custódia master não encontrada.");
    }

    public async Task AdicionarAsync(CustodiaMaster custodia, CancellationToken ct = default) =>
        await _context.CustodiasMaster.AddAsync(custodia, ct);

    public Task AtualizarAsync(CustodiaMaster custodia, CancellationToken ct = default)
    {
        _context.CustodiasMaster.Update(custodia);
        return Task.CompletedTask;
    }
}
