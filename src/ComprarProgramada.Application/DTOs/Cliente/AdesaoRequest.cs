namespace ComprarProgramada.Application.DTOs.Cliente;

public sealed record AdesaoRequest(
    string Nome,
    string Cpf,
    string Email,
    decimal ValorMensal
);
