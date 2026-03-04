namespace ComprarProgramada.Application.DTOs.Cliente;

public sealed record AdesaoResponse(
    int ClienteId,
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensal,
    decimal ValorParcela,
    string NumeroConta,
    DateTime DataAdesao
);
