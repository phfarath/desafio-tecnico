using ComprarProgramada.Application;
using ComprarProgramada.API.Middleware;
using ComprarProgramada.Infrastructure;
using ComprarProgramada.Infrastructure.Initialization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Compra Programada de Ações — Itaú Corretora",
        Version = "v1",
        Description = "API para gestão de compras programadas de ações.\n\n" +
                      "**Fluxo básico:**\n" +
                      "1. `POST /api/admin/cesta` — cadastre a cesta Top Five\n" +
                      "2. `POST /api/clientes/adesao` — cadastre clientes\n" +
                      "3. `POST /api/motor/executar-compra` — execute a compra programada"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.EnsureSeedDataAsync();

app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.DisplayRequestDuration());
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }
