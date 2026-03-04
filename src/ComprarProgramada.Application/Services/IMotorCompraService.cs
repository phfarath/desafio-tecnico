using ComprarProgramada.Application.DTOs.Motor;

namespace ComprarProgramada.Application.Services;

public interface IMotorCompraService
{
    /// <summary>
    /// Executa o ciclo completo de compra programada para a data informada.
    /// </summary>
    Task<ExecutarCompraResponse> ExecutarAsync(DateOnly dataCompra, CancellationToken ct = default);
}
