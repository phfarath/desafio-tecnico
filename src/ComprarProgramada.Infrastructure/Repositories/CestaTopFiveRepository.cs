using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprarProgramada.Infrastructure.Repositories;

public class CestaTopFiveRepository : ICestaTopFiveRepository
{
    private readonly AppDbContext _context;

    public CestaTopFiveRepository(AppDbContext context) => _context = context;

    public async Task<CestaTopFive?> ObterAtivaAsync(CancellationToken ct = default) =>
        await _context.CestasTopFive
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Ativa, ct);

    public async Task<CestaTopFive?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _context.CestasTopFive
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CestaTopFive>> ObterHistoricoAsync(CancellationToken ct = default) =>
        await _context.CestasTopFive
            .Include(c => c.Itens)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(ct);

    public async Task AdicionarAsync(CestaTopFive cesta, CancellationToken ct = default) =>
        await _context.CestasTopFive.AddAsync(cesta, ct);

    public Task AtualizarAsync(CestaTopFive cesta, CancellationToken ct = default)
    {
        _context.CestasTopFive.Update(cesta);
        return Task.CompletedTask;
    }
}
