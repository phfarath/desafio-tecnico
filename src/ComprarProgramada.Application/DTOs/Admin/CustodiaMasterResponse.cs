namespace ComprarProgramada.Application.DTOs.Admin;

public sealed record CustodiaMasterResponse(
    IReadOnlyList<CustodiaMasterItemResponse> Itens,
    decimal ValorTotalResiduo);

public sealed record CustodiaMasterItemResponse(
    string Ticker,
    int Quantidade,
    decimal PrecoMedio,
    decimal ValorTotal);
