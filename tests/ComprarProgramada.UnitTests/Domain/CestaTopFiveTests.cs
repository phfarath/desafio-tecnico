using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Exceptions;
using FluentAssertions;

namespace ComprarProgramada.UnitTests.Domain;

public class CestaTopFiveTests
{
    private static IEnumerable<(string, decimal)> Composicao5Valida() =>
    [
        ("PETR4", 25m), ("VALE3", 25m), ("ITUB4", 20m), ("BBDC4", 15m), ("ABEV3", 15m)
    ];

    [Fact]
    public void Criar_ComposicaoValida_DeveCriarCestaAtiva()
    {
        var cesta = CestaTopFive.Criar("Top Five Jan", Composicao5Valida());

        cesta.Nome.Should().Be("Top Five Jan");
        cesta.Ativa.Should().BeTrue();
        cesta.Itens.Should().HaveCount(5);
        cesta.Itens.Sum(i => i.Percentual).Should().Be(100m);
    }

    [Fact]
    public void Criar_MenosQuincoAtivos_DeveLancarDomainException()
    {
        var composicao = new[] { ("PETR4", 60m), ("VALE3", 40m) };

        var act = () => CestaTopFive.Criar("Inválida", composicao);

        act.Should().Throw<DomainException>().WithMessage("*5 ativos*");
    }

    [Fact]
    public void Criar_SomaDiferenteDe100_DeveLancarDomainException()
    {
        var composicao = new[]
        {
            ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 10m)
        };

        var act = () => CestaTopFive.Criar("Inválida", composicao);

        act.Should().Throw<DomainException>().WithMessage("*100%*");
    }

    [Fact]
    public void Criar_TickersDuplicados_DeveLancarDomainException()
    {
        var composicao = new[]
        {
            ("PETR4", 20m), ("PETR4", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m)
        };

        var act = () => CestaTopFive.Criar("Inválida", composicao);

        act.Should().Throw<DomainException>().WithMessage("*duplicados*");
    }

    [Fact]
    public void Desativar_CestaAtiva_DeveDefinirAtivaFalseEDataDesativacao()
    {
        var cesta = CestaTopFive.Criar("Top Five", Composicao5Valida());

        cesta.Desativar();

        cesta.Ativa.Should().BeFalse();
        cesta.DataDesativacao.Should().NotBeNull();
    }

    [Fact]
    public void Desativar_CestaJaInativa_DeveLancarDomainException()
    {
        var cesta = CestaTopFive.Criar("Top Five", Composicao5Valida());
        cesta.Desativar();

        var act = () => cesta.Desativar();

        act.Should().Throw<DomainException>().WithMessage("*inativa*");
    }

    [Fact]
    public void Itens_FracaoDecimal_DeveSerPercentualDivididoPorCem()
    {
        var cesta = CestaTopFive.Criar("Top Five", Composicao5Valida());

        var petr4 = cesta.Itens.First(i => i.Ticker.Valor == "PETR4");

        petr4.FracaoDecimal.Should().Be(0.25m);
    }
}
