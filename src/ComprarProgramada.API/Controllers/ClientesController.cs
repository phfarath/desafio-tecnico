using ComprarProgramada.Application.DTOs.Carteira;
using ComprarProgramada.Application.DTOs.Cliente;
using ComprarProgramada.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComprarProgramada.API.Controllers;

[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
public sealed class ClientesController(
    IClienteService clienteService,
    ICarteiraService carteiraService) : ControllerBase
{
    /// <summary>Realiza adesão de um novo cliente ao programa de compra programada.</summary>
    [HttpPost("adesao")]
    [ProducesResponseType<AdesaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Aderir([FromBody] AdesaoRequest request, CancellationToken ct)
    {
        var response = await clienteService.AderirAsync(request, ct);
        return CreatedAtAction(nameof(ObterCarteira), new { id = response.ClienteId }, response);
    }

    /// <summary>Altera o valor mensal de aporte de um cliente.</summary>
    [HttpPut("{id:int}/valor-mensal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarValorMensal(
        int id, [FromBody] AlterarValorMensalRequest request, CancellationToken ct)
    {
        await clienteService.AlterarValorMensalAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Solicita a saída do cliente do programa de compra programada.</summary>
    [HttpPost("{id:int}/saida")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Saida(int id, CancellationToken ct)
    {
        await clienteService.DesativarAsync(id, ct);
        return NoContent();
    }

    /// <summary>Retorna a carteira atual de um cliente com posições e rentabilidade.</summary>
    [HttpGet("{id:int}/carteira")]
    [ProducesResponseType<CarteiraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCarteira(int id, CancellationToken ct)
    {
        var carteira = await carteiraService.ObterCarteiraAsync(id, ct);
        return Ok(carteira);
    }

    /// <summary>Retorna o histórico de aportes e a evolução da carteira de um cliente.</summary>
    [HttpGet("{id:int}/rentabilidade")]
    [ProducesResponseType<RentabilidadeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterRentabilidade(int id, CancellationToken ct)
    {
        var rentabilidade = await carteiraService.ObterRentabilidadeAsync(id, ct);
        return Ok(rentabilidade);
    }
}
