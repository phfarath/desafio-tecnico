using ComprarProgramada.Application.DTOs.Cesta;

namespace ComprarProgramada.Application.Services;

public interface ICestaService
{
    Task<CestaResponse> CriarCestaAsync(CriarCestaRequest request, CancellationToken ct = default);
    Task<CestaResponse> ObterCestaAtivaAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CestaResponse>> ObterHistoricoAsync(CancellationToken ct = default);
}
