using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ComprarProgramada.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ContaMaster> ContasMaster => Set<ContaMaster>();
    public DbSet<ContaFilhote> ContasFilhote => Set<ContaFilhote>();
    public DbSet<CustodiaFilhote> CustodiasFilhote => Set<CustodiaFilhote>();
    public DbSet<CustodiaFilhoteItem> CustodiasFilhoteItens => Set<CustodiaFilhoteItem>();
    public DbSet<CustodiaMaster> CustodiasMaster => Set<CustodiaMaster>();
    public DbSet<CustodiaMasterItem> CustodiasMasterItens => Set<CustodiaMasterItem>();
    public DbSet<CestaTopFive> CestasTopFive => Set<CestaTopFive>();
    public DbSet<CestaItem> CestaItens => Set<CestaItem>();
    public DbSet<OrdemCompra> OrdensCompra => Set<OrdemCompra>();
    public DbSet<OrdemCompraItem> OrdensCompraItens => Set<OrdemCompraItem>();
    public DbSet<Distribuicao> Distribuicoes => Set<Distribuicao>();
    public DbSet<DistribuicaoItem> DistribuicaoItens => Set<DistribuicaoItem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Register global value converters so EF Core does not treat Value Objects
        // (sealed records) as complex/entity types.
        configurationBuilder.Properties<Ticker>().HaveConversion<TickerConverter>();
        configurationBuilder.Properties<Cpf>().HaveConversion<CpfConverter>();
        configurationBuilder.Properties<Email>().HaveConversion<EmailConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

file sealed class TickerConverter() : ValueConverter<Ticker, string>(t => t.Valor, s => Ticker.Criar(s));
file sealed class CpfConverter() : ValueConverter<Cpf, string>(c => c.Valor, s => Cpf.Criar(s));
file sealed class EmailConverter() : ValueConverter<Email, string>(e => e.Valor, s => Email.Criar(s));
