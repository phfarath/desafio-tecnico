using ComprarProgramada.Application;
using ComprarProgramada.Infrastructure;
using ComprarProgramada.Infrastructure.Initialization;
using ComprarProgramada.Worker.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ComprasProgramadasJob");

    q.AddJob<ComprasProgramadasJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ComprasProgramadasJob-trigger")
        .WithDescription("Executa às 10h nos dias 5, 15 e 25 de cada mês")
        // Cron: segundos minutos horas dia-mes mes dia-semana
        // "0 0 10 5,15,25 * ?" = todo mês, dias 5/15/25, às 10:00:00
        .WithCronSchedule("0 0 10 5,15,25 * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var host = builder.Build();
await host.Services.EnsureSeedDataAsync();
await host.RunAsync();
