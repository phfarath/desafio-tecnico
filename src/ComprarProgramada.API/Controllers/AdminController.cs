using ComprarProgramada.Application.DTOs.Cesta;
using ComprarProgramada.Application.DTOs.Rebalanceamento;
using ComprarProgramada.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComprarProgramada.API.Controllers;

[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public sealed class AdminController(
    ICestaService cestaService,
    IRebalanceamentoService rebalanceamentoService) : ControllerBase
{
    /// <summary>
    /// Cria uma nova cesta Top Five, desativando a anterior.
    /// A cesta deve conter exatamente 5 ativos e a soma dos percentuais deve ser 100%.
    /// </summary>
    [HttpPost("cesta")]
    [ProducesResponseType<CestaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarCesta([FromBody] CriarCestaRequest request, CancellationToken ct)
    {
        var response = await cestaService.CriarCestaAsync(request, ct);
        return CreatedAtAction(nameof(ObterCestaAtiva), null, response);
    }

    /// <summary>Retorna a cesta Top Five atualmente ativa.</summary>
    [HttpGet("cesta/ativa")]
    [ProducesResponseType<CestaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCestaAtiva(CancellationToken ct)
    {
        var response = await cestaService.ObterCestaAtivaAsync(ct);
        return Ok(response);
    }

    /// <summary>
    /// Executa manualmente o rebalanceamento de carteiras ao trocar a cesta Top Five.
    /// Normalmente disparado automaticamente ao criar uma nova cesta.
    /// </summary>
    [HttpPost("rebalanceamento/executar")]
    [ProducesResponseType<RebalanceamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecutarRebalanceamento(
        [FromQuery] int cestaAnteriorId,
        [FromQuery] int novaCestaId,
        CancellationToken ct)
    {
        var response = await rebalanceamentoService.ExecutarAsync(cestaAnteriorId, novaCestaId, ct);
        return Ok(response);
    }
}
