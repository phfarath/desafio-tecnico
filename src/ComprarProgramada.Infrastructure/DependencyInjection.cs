using ComprarProgramada.Domain.Interfaces;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.Interfaces.Services;
using ComprarProgramada.Infrastructure.Cotahist;
using ComprarProgramada.Infrastructure.Messaging;
using ComprarProgramada.Infrastructure.Persistence;
using ComprarProgramada.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ComprarProgramada.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionString 'DefaultConnection' não encontrada em appsettings.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysql =>
                {
                    mysql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    mysql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

        // Cotação (COTAHIST)
        services.AddSingleton<ICotacaoService, CotacaoService>();

        // Kafka
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        services.AddSingleton<IEventPublisher, KafkaProducer>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ICestaTopFiveRepository, CestaTopFiveRepository>();
        services.AddScoped<IContaMasterRepository, ContaMasterRepository>();
        services.AddScoped<IContaFilhoteRepository, ContaFilhoteRepository>();
        services.AddScoped<ICustodiaFilhoteRepository, CustodiaFilhoteRepository>();
        services.AddScoped<ICustodiaMasterRepository, CustodiaMasterRepository>();
        services.AddScoped<IOrdemCompraRepository, OrdemCompraRepository>();
        services.AddScoped<IDistribuicaoRepository, DistribuicaoRepository>();

        return services;
    }
}
