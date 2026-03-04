namespace ComprarProgramada.Application.DTOs.Cesta;

public sealed record CestaResponse(
    int Id,
    string Nome,
    bool Ativa,
    DateTime DataCriacao,
    DateTime? DataDesativacao,
    IReadOnlyList<CestaItemResponse> Itens);

public sealed record CestaItemResponse(string Ticker, decimal Percentual);
