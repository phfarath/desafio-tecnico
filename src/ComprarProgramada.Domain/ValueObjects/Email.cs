using ComprarProgramada.Domain.Exceptions;

namespace ComprarProgramada.Domain.ValueObjects;

public sealed record Email
{
    public string Valor { get; }

    private Email(string valor) => Valor = valor;

    public static Email Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || !valor.Contains('@') || valor.Trim().Length < 5)
            throw new DomainException($"Email inválido: '{valor}'.");
        return new Email(valor.Trim().ToLowerInvariant());
    }

    public override string ToString() => Valor;
}
