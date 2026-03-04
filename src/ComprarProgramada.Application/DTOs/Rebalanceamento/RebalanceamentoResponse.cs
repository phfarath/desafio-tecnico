namespace ComprarProgramada.Application.DTOs.Rebalanceamento;

public sealed record RebalanceamentoResponse(
    int TotalClientes,
    int TotalClientesComVendas,
    decimal TotalVendasGeral,
    decimal TotalIrPublicado,
    IReadOnlyList<RebalanceamentoClienteResponse> Detalhes);

public sealed record RebalanceamentoClienteResponse(
    int ClienteId,
    string NomeCliente,
    decimal TotalVendas,
    decimal LucroLiquido,
    bool IrPublicado,
    decimal ValorIr);
