using ComprarProgramada.Application.DTOs.Cliente;

namespace ComprarProgramada.Application.Services;

public interface IClienteService
{
    Task<AdesaoResponse> AderirAsync(AdesaoRequest request, CancellationToken ct = default);
    Task AlterarValorMensalAsync(int clienteId, AlterarValorMensalRequest request, CancellationToken ct = default);
    Task DesativarAsync(int clienteId, CancellationToken ct = default);
}
