using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.ValueObjects;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComprarProgramada.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context) => _context = context;

    public async Task<Cliente?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _context.Clientes
            .Include(c => c.ContaFilhote)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Cliente?> ObterPorCpfAsync(Cpf cpf, CancellationToken ct = default) =>
        await _context.Clientes
            .FirstOrDefaultAsync(c => c.Cpf == cpf, ct);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(
        bool? ativo = null,
        string? nome = null,
        CancellationToken ct = default)
    {
        var query = _context.Clientes
            .Include(c => c.ContaFilhote)
            .AsQueryable();

        if (ativo.HasValue)
            query = query.Where(c => c.Ativo == ativo.Value);

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var termo = nome.Trim();
            query = query.Where(c => c.Nome.Contains(termo));
        }

        return await query
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Cliente>> ObterAtivosAsync(CancellationToken ct = default) =>
        await _context.Clientes
            .Include(c => c.ContaFilhote)
            .Where(c => c.Ativo)
            .ToListAsync(ct);

    public async Task<bool> ExisteCpfAsync(Cpf cpf, CancellationToken ct = default) =>
        await _context.Clientes
            .AnyAsync(c => c.Cpf == cpf, ct);

    public async Task AdicionarAsync(Cliente cliente, CancellationToken ct = default) =>
        await _context.Clientes.AddAsync(cliente, ct);

    public Task AtualizarAsync(Cliente cliente, CancellationToken ct = default)
    {
        _context.Clientes.Update(cliente);
        return Task.CompletedTask;
    }
}
