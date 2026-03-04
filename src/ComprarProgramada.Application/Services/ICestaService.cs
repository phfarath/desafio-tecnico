using ComprarProgramada.Application.DTOs.Cesta;

namespace ComprarProgramada.Application.Services;

public interface ICestaService
{
    Task<CestaResponse> CriarCestaAsync(CriarCestaRequest request, CancellationToken ct = default);
    Task<CestaResponse> ObterCestaAtivaAsync(CancellationToken ct = default);
}
