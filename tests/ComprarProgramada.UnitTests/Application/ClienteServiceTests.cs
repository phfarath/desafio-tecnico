using ComprarProgramada.Application.DTOs.Cliente;
using ComprarProgramada.Application.Services;
using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ComprarProgramada.UnitTests.Application;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly Mock<IContaFilhoteRepository> _contaRepo = new();
    private readonly Mock<ICustodiaFilhoteRepository> _custodiaRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private ClienteService CriarService() =>
        new(_clienteRepo.Object, _contaRepo.Object, _custodiaRepo.Object, _uow.Object);

    [Fact]
    public async Task AderirAsync_CpfNovoCadastro_DeveCriarClienteContaECustodia()
    {
        _clienteRepo.Setup(r => r.ExisteCpfAsync(It.IsAny<Cpf>(), default)).ReturnsAsync(false);
        _contaRepo.Setup(r => r.ObterProximoSequencialAsync(default)).ReturnsAsync(1);

        var request = new AdesaoRequest("João Silva", "529.982.247-25", "joao@email.com", 300m);
        var service = CriarService();

        var result = await service.AderirAsync(request);

        result.Nome.Should().Be("João Silva");
        result.ValorMensal.Should().Be(300m);
        result.ValorParcela.Should().Be(100m);

        _clienteRepo.Verify(r => r.AdicionarAsync(It.IsAny<Cliente>(), default), Times.Once);
        _contaRepo.Verify(r => r.AdicionarAsync(It.IsAny<ContaFilhote>(), default), Times.Once);
        _custodiaRepo.Verify(r => r.AdicionarAsync(It.IsAny<CustodiaFilhote>(), default), Times.Once);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task AderirAsync_CpfDuplicado_DeveLancarInvalidOperationException()
    {
        _clienteRepo.Setup(r => r.ExisteCpfAsync(It.IsAny<Cpf>(), default)).ReturnsAsync(true);

        var request = new AdesaoRequest("João", "529.982.247-25", "joao@email.com", 300m);
        var service = CriarService();

        var act = async () => await service.AderirAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*já cadastrado*");
        _clienteRepo.Verify(r => r.AdicionarAsync(It.IsAny<Cliente>(), default), Times.Never);
    }

    [Fact]
    public async Task AlterarValorMensalAsync_ClienteExistente_DeveAtualizarECommitar()
    {
        var cliente = Cliente.Criar("João", "529.982.247-25", "joao@email.com", 300m);
        _clienteRepo.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(cliente);

        var service = CriarService();
        await service.AlterarValorMensalAsync(1, new AlterarValorMensalRequest(600m));

        cliente.ValorMensal.Should().Be(600m);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task AlterarValorMensalAsync_ClienteNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        _clienteRepo.Setup(r => r.ObterPorIdAsync(99, default)).ReturnsAsync((Cliente?)null);

        var service = CriarService();
        var act = async () => await service.AlterarValorMensalAsync(99, new AlterarValorMensalRequest(600m));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DesativarAsync_ClienteExistente_DeveDesativarECommitar()
    {
        var cliente = Cliente.Criar("João", "529.982.247-25", "joao@email.com", 300m);
        _clienteRepo.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(cliente);

        var service = CriarService();
        await service.DesativarAsync(1);

        cliente.Ativo.Should().BeFalse();
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }
}
