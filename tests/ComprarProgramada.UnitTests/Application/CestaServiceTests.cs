using ComprarProgramada.Application.DTOs.Cesta;
using ComprarProgramada.Application.Services;
using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces;
using ComprarProgramada.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ComprarProgramada.UnitTests.Application;

public class CestaServiceTests
{
    private readonly Mock<ICestaTopFiveRepository> _cestaRepo = new();
    private readonly Mock<IRebalanceamentoService> _rebalanceamento = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private CestaService CriarService() =>
        new(_cestaRepo.Object, _rebalanceamento.Object, _uow.Object);

    private static CriarCestaRequest RequestValida(string nome = "Top Five") =>
        new(nome,
        [
            new("PETR4", 25m), new("VALE3", 25m), new("ITUB4", 20m),
            new("BBDC4", 15m), new("ABEV3", 15m)
        ]);

    [Fact]
    public async Task CriarCestaAsync_PrimeiraCesta_DeveCriarSemDisparoDeRebalanceamento()
    {
        _cestaRepo.Setup(r => r.ObterAtivaAsync(default)).ReturnsAsync((CestaTopFive?)null);

        var service = CriarService();
        var result = await service.CriarCestaAsync(RequestValida());

        result.Nome.Should().Be("Top Five");
        result.Ativa.Should().BeTrue();
        result.Itens.Should().HaveCount(5);

        _rebalanceamento.Verify(r =>
            r.ExecutarAsync(It.IsAny<int>(), It.IsAny<int>(), default), Times.Never);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task CriarCestaAsync_ComCestaAnterior_DeveDesativarAnteriorEDispararRebalanceamento()
    {
        var cestaAnterior = CestaTopFive.Criar("Top Five Anterior",
        [
            ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m)
        ]);
        _cestaRepo.Setup(r => r.ObterAtivaAsync(default)).ReturnsAsync(cestaAnterior);

        var service = CriarService();
        await service.CriarCestaAsync(RequestValida("Top Five Nova"));

        cestaAnterior.Ativa.Should().BeFalse();
        _rebalanceamento.Verify(r =>
            r.ExecutarAsync(cestaAnterior.Id, It.IsAny<int>(), default), Times.Once);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ObterCestaAtivaAsync_SemCestaAtiva_DeveLancarInvalidOperationException()
    {
        _cestaRepo.Setup(r => r.ObterAtivaAsync(default)).ReturnsAsync((CestaTopFive?)null);

        var service = CriarService();
        var act = async () => await service.ObterCestaAtivaAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*ativa*");
    }
}
