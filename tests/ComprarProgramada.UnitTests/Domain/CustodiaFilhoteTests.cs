using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Exceptions;
using ComprarProgramada.Domain.ValueObjects;
using FluentAssertions;

namespace ComprarProgramada.UnitTests.Domain;

public class CustodiaFilhoteTests
{
    private static Ticker Petr4 => Ticker.Criar("PETR4");
    private static Ticker Vale3 => Ticker.Criar("VALE3");

    [Fact]
    public void AdicionarAtivos_NovoTicker_DeveCriarItemComPrecoMedioCorreto()
    {
        var custodia = CustodiaFilhote.Criar(1);

        custodia.AdicionarAtivos(Petr4, 100, 32.50m);

        var item = custodia.ObterItem(Petr4)!;
        item.Quantidade.Should().Be(100);
        item.PrecoMedio.Should().Be(32.50m);
    }

    [Fact]
    public void AdicionarAtivos_TickerExistente_DeveRecalcularPrecoMedioPonderado()
    {
        var custodia = CustodiaFilhote.Criar(1);
        custodia.AdicionarAtivos(Petr4, 100, 30m);  // PM inicial = 30

        custodia.AdicionarAtivos(Petr4, 100, 40m);  // novo lote PM = 40

        // PM esperado = (100*30 + 100*40) / 200 = 35
        var item = custodia.ObterItem(Petr4)!;
        item.Quantidade.Should().Be(200);
        item.PrecoMedio.Should().Be(35m);
    }

    [Fact]
    public void RemoverAtivos_QuantidadeTotal_DeveRemoverItemDaLista()
    {
        var custodia = CustodiaFilhote.Criar(1);
        custodia.AdicionarAtivos(Petr4, 100, 30m);

        custodia.RemoverAtivos(Petr4, 100);

        custodia.ObterItem(Petr4).Should().BeNull();
        custodia.Itens.Should().BeEmpty();
    }

    [Fact]
    public void RemoverAtivos_QuantidadeParcial_DeveManterItemComQuantidadeResidual()
    {
        var custodia = CustodiaFilhote.Criar(1);
        custodia.AdicionarAtivos(Petr4, 200, 30m);

        custodia.RemoverAtivos(Petr4, 50);

        custodia.ObterItem(Petr4)!.Quantidade.Should().Be(150);
    }

    [Fact]
    public void RemoverAtivos_QuantidadeInsuficiente_DeveLancarDomainException()
    {
        var custodia = CustodiaFilhote.Criar(1);
        custodia.AdicionarAtivos(Petr4, 10, 30m);

        var act = () => custodia.RemoverAtivos(Petr4, 50);

        act.Should().Throw<DomainException>().WithMessage("*insuficiente*");
    }

    [Fact]
    public void RemoverAtivos_TickerInexistente_DeveLancarDomainException()
    {
        var custodia = CustodiaFilhote.Criar(1);

        var act = () => custodia.RemoverAtivos(Vale3, 10);

        act.Should().Throw<DomainException>().WithMessage("*não encontrado*");
    }

    [Fact]
    public void PrecoMedioNaoMuda_AoRemoverAcoes()
    {
        var custodia = CustodiaFilhote.Criar(1);
        custodia.AdicionarAtivos(Petr4, 100, 40m);

        custodia.RemoverAtivos(Petr4, 50);

        // PM deve permanecer 40 mesmo após venda
        custodia.ObterItem(Petr4)!.PrecoMedio.Should().Be(40m);
    }

    [Fact]
    public void PossuiAtivo_TickerExistente_DeveRetornarTrue()
    {
        var custodia = CustodiaFilhote.Criar(1);
        custodia.AdicionarAtivos(Petr4, 10, 30m);

        custodia.PossuiAtivo(Petr4).Should().BeTrue();
        custodia.PossuiAtivo(Vale3).Should().BeFalse();
    }
}
