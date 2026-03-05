using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComprarProgramada.Infrastructure.Initialization;

public static class DatabaseSeedExtensions
{
    public static async Task EnsureSeedDataAsync(this IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var contaMaster = await db.ContasMaster.FirstOrDefaultAsync(ct);
        if (contaMaster is null)
        {
            contaMaster = ContaMaster.Criar();
            await db.ContasMaster.AddAsync(contaMaster, ct);
            await db.SaveChangesAsync(ct);
        }

        var possuiCustodiaMaster = await db.CustodiasMaster
            .AnyAsync(c => c.ContaMasterId == contaMaster.Id, ct);

        if (!possuiCustodiaMaster)
        {
            var custodiaMaster = CustodiaMaster.Criar(contaMaster.Id);
            await db.CustodiasMaster.AddAsync(custodiaMaster, ct);
            await db.SaveChangesAsync(ct);
        }
    }
}
