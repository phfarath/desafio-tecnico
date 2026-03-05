using ComprarProgramada.Application.DTOs.Admin;
using ComprarProgramada.Application.DTOs.Cesta;
using ComprarProgramada.Application.DTOs.Rebalanceamento;
using ComprarProgramada.Application.Services;
using ComprarProgramada.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ComprarProgramada.API.Controllers;

[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public sealed class AdminController(
    ICestaService cestaService,
    IRebalanceamentoService rebalanceamentoService,
    ICustodiaMasterRepository custodiaMasterRepository) : ControllerBase
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
        return CreatedAtAction(nameof(ObterCestaAtual), null, response);
    }

    /// <summary>Retorna a cesta Top Five atualmente ativa.</summary>
    [HttpGet("cesta/atual")]
    [ProducesResponseType<CestaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCestaAtual(CancellationToken ct)
    {
        var response = await cestaService.ObterCestaAtivaAsync(ct);
        return Ok(response);
    }

    /// <summary>Retorna o histórico de todas as cestas Top Five (ativas e inativas).</summary>
    [HttpGet("cesta/historico")]
    [ProducesResponseType<IReadOnlyList<CestaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterHistoricoCestas(CancellationToken ct)
    {
        var response = await cestaService.ObterHistoricoAsync(ct);
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

    /// <summary>
    /// RN-050: Rebalanceia carteiras cujo desvio de proporção supera o limiar informado.
    /// O limiar padrão é 5 pontos percentuais.
    /// </summary>
    [HttpPost("rebalanceamento/executar-desvio")]
    [ProducesResponseType<RebalanceamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecutarRebalanceamentoPorDesvio(
        [FromQuery] decimal limiarDesvio = 5m,
        CancellationToken ct = default)
    {
        var response = await rebalanceamentoService.ExecutarPorDesvioAsync(limiarDesvio, ct);
        return Ok(response);
    }

    /// <summary>Retorna os resíduos atuais da custódia da conta master.</summary>
    [HttpGet("conta-master/custodia")]
    [ProducesResponseType<CustodiaMasterResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterCustodiaMaster(CancellationToken ct)
    {
        var custodia = await custodiaMasterRepository.ObterComItensAsync(ct);

        var itens = custodia.Itens
            .Select(i => new CustodiaMasterItemResponse(
                i.Ticker.Valor,
                i.Quantidade,
                Math.Round(i.PrecoMedio, 2),
                Math.Round(i.Quantidade * i.PrecoMedio, 2)))
            .ToList();

        var valorTotal = itens.Sum(i => i.ValorTotal);

        return Ok(new CustodiaMasterResponse(itens, Math.Round(valorTotal, 2)));
    }
}
