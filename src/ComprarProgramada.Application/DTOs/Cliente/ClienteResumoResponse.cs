namespace ComprarProgramada.Application.DTOs.Cliente;

public sealed record ClienteResumoResponse(
    int ClienteId,
    string Nome,
    string CpfMascarado,
    bool Ativo,
    decimal ValorMensal,
    string NumeroConta,
    DateTime DataAdesao);
