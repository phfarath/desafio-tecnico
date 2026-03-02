using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprarProgramada.Infrastructure.Repositories;

public class DistribuicaoRepository : IDistribuicaoRepository
{
    private readonly AppDbContext _context;

    public DistribuicaoRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Distribuicao>> ObterPorOrdemAsync(int ordemCompraId, CancellationToken ct = default) =>
        await _context.Distribuicoes
            .Include(d => d.Itens)
            .Where(d => d.OrdemCompraId == ordemCompraId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Distribuicao>> ObterPorClienteAsync(int clienteId, CancellationToken ct = default) =>
        await _context.Distribuicoes
            .Include(d => d.Itens)
            .Where(d => d.ClienteId == clienteId)
            .OrderByDescending(d => d.DataDistribuicao)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Distribuicao distribuicao, CancellationToken ct = default) =>
        await _context.Distribuicoes.AddAsync(distribuicao, ct);

    public async Task AdicionarVariasAsync(IEnumerable<Distribuicao> distribuicoes, CancellationToken ct = default) =>
        await _context.Distribuicoes.AddRangeAsync(distribuicoes, ct);
}
