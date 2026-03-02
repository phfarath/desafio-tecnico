using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprarProgramada.Infrastructure.Repositories;

public class ContaMasterRepository : IContaMasterRepository
{
    private readonly AppDbContext _context;

    public ContaMasterRepository(AppDbContext context) => _context = context;

    public async Task<ContaMaster?> ObterAsync(CancellationToken ct = default) =>
        await _context.ContasMaster.FirstOrDefaultAsync(ct);

    public async Task<ContaMaster> ObterComCustodiaAsync(CancellationToken ct = default)
    {
        var conta = await _context.ContasMaster
            .Include(c => c.Custodia!)
                .ThenInclude(cm => cm.Itens)
            .FirstOrDefaultAsync(ct);

        return conta ?? throw new DomainException("Conta master não encontrada. Verifique a inicialização do sistema.");
    }

    public async Task AdicionarAsync(ContaMaster conta, CancellationToken ct = default) =>
        await _context.ContasMaster.AddAsync(conta, ct);

    public Task AtualizarAsync(ContaMaster conta, CancellationToken ct = default)
    {
        _context.ContasMaster.Update(conta);
        return Task.CompletedTask;
    }
}
