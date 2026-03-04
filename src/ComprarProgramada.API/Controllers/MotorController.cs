using ComprarProgramada.Application.DTOs.Motor;
using ComprarProgramada.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComprarProgramada.API.Controllers;

[ApiController]
[Route("api/motor")]
[Produces("application/json")]
public sealed class MotorController(IMotorCompraService motorCompraService) : ControllerBase
{
    /// <summary>
    /// Executa manualmente o motor de compra programada para uma data específica.
    /// Em produção este endpoint é acionado automaticamente pelo Quartz Job nos dias 5, 15 e 25.
    /// </summary>
    [HttpPost("executar-compra")]
    [ProducesResponseType<ExecutarCompraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ExecutarCompra(
        [FromQuery] DateOnly? data, CancellationToken ct)
    {
        var dataCompra = data ?? DateOnly.FromDateTime(DateTime.Today);
        var response = await motorCompraService.ExecutarAsync(dataCompra, ct);
        return Ok(response);
    }
}
