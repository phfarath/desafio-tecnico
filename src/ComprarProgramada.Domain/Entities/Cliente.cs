using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Domain.Entities;

public class Cliente : Entity
{
    public const decimal ValorMensalMinimo = 100m;

    public string Nome { get; private set; } = string.Empty;
    public Cpf Cpf { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public decimal ValorMensal { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataAdesao { get; private set; }
    public DateTime? DataSaida { get; private set; }

    // Navigation
    public ContaFilhote? ContaFilhote { get; private set; }

    protected Cliente() { }

    private Cliente(string nome, Cpf cpf, Email email, decimal valorMensal)
    {
        Nome = nome;
        Cpf = cpf;
        Email = email;
        ValidarValorMensal(valorMensal);
        ValorMensal = valorMensal;
        Ativo = true;
        DataAdesao = DateTime.UtcNow;
    }

    public static Cliente Criar(string nome, string cpf, string email, decimal valorMensal)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório.");

        return new Cliente(
            nome.Trim(),
            Cpf.Criar(cpf),
            Email.Criar(email),
            valorMensal);
    }

    public void AlterarValorMensal(decimal novoValor)
    {
        if (!Ativo)
            throw new DomainException("Não é possível alterar o valor mensal de um cliente inativo.");

        ValidarValorMensal(novoValor);
        ValorMensal = novoValor;
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new DomainException("Cliente já está inativo.");

        Ativo = false;
        DataSaida = DateTime.UtcNow;
    }

    public void AssociarContaFilhote(ContaFilhote conta)
    {
        if (ContaFilhote is not null)
            throw new DomainException("Cliente já possui uma conta gráfica associada.");

        ContaFilhote = conta;
    }

    /// <summary>Valor do aporte por data de compra (1/3 do mensal).</summary>
    public decimal ValorParcela => ValorMensal / 3m;

    private static void ValidarValorMensal(decimal valor)
    {
        if (valor < ValorMensalMinimo)
            throw new DomainException(
                $"O valor mensal mínimo é de R$ {ValorMensalMinimo:N2}.");
    }
}
