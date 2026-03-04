using ComprarProgramada.Application.Services;
using Quartz;

namespace ComprarProgramada.Worker.Jobs;

/// <summary>
/// Job Quartz que executa o motor de compra programada nos dias 5, 15 e 25 de cada mês.
/// Caso o dia caia em fim de semana, aguarda o primeiro dia útil seguinte.
/// </summary>
[DisallowConcurrentExecution]
public sealed class ComprasProgramadasJob(
    IMotorCompraService motorCompraService,
    ILogger<ComprasProgramadasJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        if (!EhDiaExecucao(hoje))
        {
            logger.LogInformation(
                "ComprasProgramadasJob: {Data} não é dia de execução (5/15/25 ou 1° útil seguinte). Ignorando.",
                hoje);
            return;
        }

        logger.LogInformation("ComprasProgramadasJob: iniciando execução para {Data}.", hoje);

        try
        {
            var resultado = await motorCompraService.ExecutarAsync(hoje, context.CancellationToken);

            logger.LogInformation(
                "ComprasProgramadasJob: concluído. OrdemId={OrdemId} | Clientes={Clientes} | " +
                "ValorTotal={Valor:C} | Ativos={Ativos} | Residuos={Residuos}",
                resultado.OrdemCompraId,
                resultado.TotalClientes,
                resultado.ValorTotalConsolidado,
                resultado.ItensComprados.Count,
                resultado.ResiduosMaster.Count);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Já existe"))
        {
            // Idempotência: job disparou duas vezes para a mesma data (não é erro)
            logger.LogWarning("ComprasProgramadasJob: compra para {Data} já foi executada anteriormente. {Msg}",
                hoje, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ComprasProgramadasJob: erro ao executar compra para {Data}.", hoje);
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    /// <summary>
    /// Retorna true se hoje for o 1° dia útil >= ao alvo mensal (5, 15 ou 25).
    /// Dias úteis: segunda a sexta (feriados não considerados nesta versão).
    /// </summary>
    private static bool EhDiaExecucao(DateOnly hoje)
    {
        int[] alvos = [5, 15, 25];

        foreach (var alvo in alvos)
        {
            var diaAlvo = new DateOnly(hoje.Year, hoje.Month, alvo);
            var primeiroDiaUtil = ProximoDiaUtil(diaAlvo);

            if (primeiroDiaUtil == hoje)
                return true;
        }

        return false;
    }

    private static DateOnly ProximoDiaUtil(DateOnly data)
    {
        while (data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            data = data.AddDays(1);
        return data;
    }
}
