using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprarProgramada.Infrastructure.Repositories;

public class OrdemCompraRepository : IOrdemCompraRepository
{
    private readonly AppDbContext _context;

    public OrdemCompraRepository(AppDbContext context) => _context = context;

    public async Task<OrdemCompra?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _context.OrdensCompra
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<OrdemCompra?> ObterPorDataAsync(DateOnly data, CancellationToken ct = default) =>
        await _context.OrdensCompra
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.DataCompra == data, ct);

    public async Task<bool> ExisteParaDataAsync(DateOnly data, CancellationToken ct = default) =>
        await _context.OrdensCompra.AnyAsync(o => o.DataCompra == data, ct);

    public async Task AdicionarAsync(OrdemCompra ordem, CancellationToken ct = default) =>
        await _context.OrdensCompra.AddAsync(ordem, ct);

    public Task AtualizarAsync(OrdemCompra ordem, CancellationToken ct = default)
    {
        _context.OrdensCompra.Update(ordem);
        return Task.CompletedTask;
    }
}
