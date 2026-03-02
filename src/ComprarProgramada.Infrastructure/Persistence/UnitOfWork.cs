using ComprarProgramada.Domain.Interfaces;

namespace ComprarProgramada.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public Task<int> CommitAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    public void Dispose() => _context.Dispose();
}
