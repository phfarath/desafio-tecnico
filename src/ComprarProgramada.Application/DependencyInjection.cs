using ComprarProgramada.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ComprarProgramada.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ICestaService, CestaService>();
        services.AddScoped<ICarteiraService, CarteiraService>();
        services.AddScoped<IMotorCompraService, MotorCompraService>();

        return services;
    }
}
