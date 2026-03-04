namespace ComprarProgramada.Application.DTOs.Cesta;

public sealed record CriarCestaRequest(
    string Nome,
    IReadOnlyList<CestaItemRequest> Itens);

public sealed record CestaItemRequest(string Ticker, decimal Percentual);
