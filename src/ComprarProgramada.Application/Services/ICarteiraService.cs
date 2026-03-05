using ComprarProgramada.Application.DTOs.Carteira;

namespace ComprarProgramada.Application.Services;

public interface ICarteiraService
{
    Task<CarteiraResponse> ObterCarteiraAsync(int clienteId, CancellationToken ct = default);
    Task<RentabilidadeResponse> ObterRentabilidadeAsync(int clienteId, CancellationToken ct = default);
}
