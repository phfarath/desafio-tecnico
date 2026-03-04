using ComprarProgramada.Application.DTOs.Rebalanceamento;

namespace ComprarProgramada.Application.Services;

public interface IRebalanceamentoService
{
    /// <summary>
    /// Executa o rebalanceamento de todos os clientes ativos ao trocar a cesta Top Five.
    /// Vende ativos que saíram da cesta, compra novos com os recursos obtidos
    /// e publica IR sobre vendas quando o total supera R$20.000 por cliente.
    /// </summary>
    Task<RebalanceamentoResponse> ExecutarAsync(
        int cestaAnteriorId,
        int novaCestaId,
        CancellationToken ct = default);
}
