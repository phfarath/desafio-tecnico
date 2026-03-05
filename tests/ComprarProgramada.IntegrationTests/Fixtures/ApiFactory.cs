using ComprarProgramada.Domain.Interfaces.Services;
using ComprarProgramada.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace ComprarProgramada.IntegrationTests.Fixtures;

public class ApiFactory : WebApplicationFactory<Program>
{
    public Mock<ICotacaoService> CotacoesMock { get; } = new();
    public Mock<IEventPublisher> EventosMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove TODAS as registrações do EF Core que envolvem AppDbContext
            // (inclui IDbContextOptionsConfiguration<AppDbContext> que chama UseMySql)
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GenericTypeArguments.Contains(typeof(AppDbContext))))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            // Registra DbContext em memória (banco único por fábrica — nome capturado antes do lambda)
            var dbName = $"integration-{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase(dbName));

            // Substitui Kafka e Cotações por mocks
            services.RemoveAll<ICotacaoService>();
            services.AddSingleton(CotacoesMock.Object);

            services.RemoveAll<IEventPublisher>();
            services.AddSingleton(EventosMock.Object);
        });
    }
}
