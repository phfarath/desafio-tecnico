using ComprarProgramada.Domain.Exceptions;

namespace ComprarProgramada.Domain.ValueObjects;

public sealed record Cpf
{
    public string Valor { get; }

    private Cpf(string valor) => Valor = valor;

    public static Cpf Criar(string valor)
    {
        var digits = new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length != 11)
            throw new DomainException("CPF inválido: deve conter exatamente 11 dígitos.");
        return new Cpf(digits);
    }

    public string Formatado =>
        $"{Valor[..3]}.{Valor[3..6]}.{Valor[6..9]}-{Valor[9..]}";

    public override string ToString() => Valor;
}
