using ComprarProgramada.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ComprarProgramada.API.Middleware;

public sealed class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleAsync(context, ex);
        }
    }

    private static Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            DomainException       => (StatusCodes.Status400BadRequest, "Erro de domínio"),
            InvalidOperationException { Message: var m } when m.Contains("já existe") || m.Contains("Já existe")
                                  => (StatusCodes.Status409Conflict, "Conflito"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Operação inválida"),
            KeyNotFoundException  => (StatusCodes.Status404NotFound, "Não encontrado"),
            _                     => (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title  = title,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode  = status;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
