namespace ComprarProgramada.Application.DTOs.Motor;

public sealed record ExecutarCompraResponse(
    int OrdemCompraId,
    DateOnly DataCompra,
    decimal ValorTotalConsolidado,
    int TotalClientes,
    IReadOnlyList<ItemCompradoResponse> ItensComprados,
    IReadOnlyList<ResidualMasterResponse> ResiduosMaster);

public sealed record ItemCompradoResponse(
    string Ticker,
    int QuantidadeLotePadrao,
    int QuantidadeFracionario,
    int QuantidadeTotal,
    decimal PrecoUnitario,
    decimal ValorTotal);

public sealed record ResidualMasterResponse(string Ticker, int Quantidade);
