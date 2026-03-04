using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Exceptions;
using FluentAssertions;

namespace ComprarProgramada.UnitTests.Domain;

public class ClienteTests
{
    [Fact]
    public void Criar_DadosValidos_DeveCriarClienteAtivo()
    {
        var cliente = Cliente.Criar("João Silva", "529.982.247-25", "joao@email.com", 300m);

        cliente.Nome.Should().Be("João Silva");
        cliente.Ativo.Should().BeTrue();
        cliente.ValorMensal.Should().Be(300m);
        cliente.ValorParcela.Should().Be(100m);
    }

    [Fact]
    public void Criar_ValorMensalAbaixoDoMinimo_DeveLancarDomainException()
    {
        var act = () => Cliente.Criar("João", "529.982.247-25", "joao@email.com", 50m);

        act.Should().Throw<DomainException>().WithMessage("*mínimo*");
    }

    [Fact]
    public void Criar_NomeVazio_DeveLancarDomainException()
    {
        var act = () => Cliente.Criar("", "529.982.247-25", "joao@email.com", 300m);

        act.Should().Throw<DomainException>().WithMessage("*Nome*");
    }

    [Fact]
    public void AlterarValorMensal_ValorValido_DeveAtualizar()
    {
        var cliente = Cliente.Criar("João", "529.982.247-25", "joao@email.com", 300m);

        cliente.AlterarValorMensal(600m);

        cliente.ValorMensal.Should().Be(600m);
    }

    [Fact]
    public void AlterarValorMensal_ClienteInativo_DeveLancarDomainException()
    {
        var cliente = Cliente.Criar("João", "529.982.247-25", "joao@email.com", 300m);
        cliente.Desativar();

        var act = () => cliente.AlterarValorMensal(600m);

        act.Should().Throw<DomainException>().WithMessage("*inativo*");
    }

    [Fact]
    public void Desativar_ClienteAtivo_DeveDefinirAtivoFalseEDataSaida()
    {
        var cliente = Cliente.Criar("João", "529.982.247-25", "joao@email.com", 300m);

        cliente.Desativar();

        cliente.Ativo.Should().BeFalse();
        cliente.DataSaida.Should().NotBeNull();
    }

    [Fact]
    public void Desativar_ClienteJaInativo_DeveLancarDomainException()
    {
        var cliente = Cliente.Criar("João", "529.982.247-25", "joao@email.com", 300m);
        cliente.Desativar();

        var act = () => cliente.Desativar();

        act.Should().Throw<DomainException>().WithMessage("*inativo*");
    }

    [Theory]
    [InlineData(300, 100)]
    [InlineData(600, 200)]
    [InlineData(150, 50)]
    public void ValorParcela_DeveSerUmTercoDoValorMensal(decimal mensal, decimal esperado)
    {
        var cliente = Cliente.Criar("João", "529.982.247-25", "joao@email.com", mensal);

        cliente.ValorParcela.Should().Be(esperado);
    }
}
