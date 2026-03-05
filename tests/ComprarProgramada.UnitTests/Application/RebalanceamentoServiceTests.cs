using ComprarProgramada.Application.Services;
using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Entities.Base;
using ComprarProgramada.Domain.Interfaces;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.Interfaces.Services;
using ComprarProgramada.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;

namespace ComprarProgramada.UnitTests.Application;

public class RebalanceamentoServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly Mock<ICestaTopFiveRepository> _cestaRepo = new();
    private readonly Mock<ICustodiaFilhoteRepository> _custodiaRepo = new();
    private readonly Mock<ICotacaoService> _cotacoes = new();
    private readonly Mock<IEventPublisher> _eventos = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private RebalanceamentoService CriarService()
    {
        var config = new ConfigurationRoot([
            new MemoryConfigurationProvider(new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string?> { ["Kafka:TopicoIrVenda"] = "ir-venda-test" }
            })
        ]);

        return new RebalanceamentoService(
            _clienteRepo.Object, _cestaRepo.Object, _custodiaRepo.Object,
            _cotacoes.Object, _eventos.Object, config, _uow.Object);
    }

    private static CestaTopFive CriarCesta(params (string ticker, decimal pct)[] itens) =>
        CestaTopFive.Criar("Cesta", itens);

    [Fact]
    public async Task ExecutarAsync_TickersIguais_DeveRetornarSemAcoes()
    {
        var composicao = new[] { ("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m) };
        var cestaAnt = CriarCesta(composicao);
        var cestaNov = CriarCesta(composicao);

        _cestaRepo.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(cestaAnt);
        _cestaRepo.Setup(r => r.ObterPorIdAsync(2, default)).ReturnsAsync(cestaNov);

        var result = await CriarService().ExecutarAsync(1, 2);

        result.TotalClientes.Should().Be(0);
        result.TotalVendasGeral.Should().Be(0);
        _clienteRepo.Verify(r => r.ObterAtivosAsync(default), Times.Never);
    }

    [Fact]
    public async Task ExecutarAsync_TickerSaindoDaCesta_DeveVenderPosicaoDoCliente()
    {
        // Cesta anterior: PETR4 incluído. Nova: MGLU3 no lugar de PETR4
        var cestaAnt = CriarCesta(("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m));
        var cestaNov = CriarCesta(("MGLU3", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m));

        _cestaRepo.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(cestaAnt);
        _cestaRepo.Setup(r => r.ObterPorIdAsync(2, default)).ReturnsAsync(cestaNov);

        // Cliente com ContaFilhote associada
        var cliente = Cliente.Criar("Ana", "529.982.247-25", "ana@email.com", 300m);
        var conta = ContaFilhote.Criar(0, 1);
        cliente.AssociarContaFilhote(conta);
        _clienteRepo.Setup(r => r.ObterAtivosAsync(default)).ReturnsAsync([cliente]);

        // Custódia com 100 PETR4 a PM=30
        var custodia = CustodiaFilhote.Criar(0);
        custodia.AdicionarAtivos(Ticker.Criar("PETR4"), 100, 30m);

        _custodiaRepo.Setup(r => r.ObterPorContaFilhoteAsync(It.IsAny<int>(), default))
            .ReturnsAsync(custodia);

        // Cotação PETR4=32, MGLU3=10
        _cotacoes.Setup(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Ticker>>(), default))
            .ReturnsAsync(new Dictionary<string, decimal> { ["PETR4"] = 32m, ["MGLU3"] = 10m });

        var result = await CriarService().ExecutarAsync(1, 2);

        result.TotalClientesComVendas.Should().Be(1);
        result.TotalVendasGeral.Should().Be(3200m); // 100 * 32
        result.Detalhes[0].LucroLiquido.Should().Be(200m); // (32-30)*100

        // PETR4 removida, MGLU3 comprada
        custodia.PossuiAtivo(Ticker.Criar("PETR4")).Should().BeFalse();
        custodia.PossuiAtivo(Ticker.Criar("MGLU3")).Should().BeTrue();

        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_VendasAcimaVinteMilReais_DevePublicarIrKafka()
    {
        var cestaAnt = CriarCesta(("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m));
        var cestaNov = CriarCesta(("MGLU3", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m));

        _cestaRepo.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(cestaAnt);
        _cestaRepo.Setup(r => r.ObterPorIdAsync(2, default)).ReturnsAsync(cestaNov);

        var cliente = Cliente.Criar("Carlos", "529.982.247-25", "carlos@email.com", 300m);
        cliente.AssociarContaFilhote(ContaFilhote.Criar(0, 1));
        _clienteRepo.Setup(r => r.ObterAtivosAsync(default)).ReturnsAsync([cliente]);

        // 1000 PETR4 a PM=20 → venda a 25 = R$25.000 (> 20k), lucro = R$5.000
        var custodia = CustodiaFilhote.Criar(0);
        custodia.AdicionarAtivos(Ticker.Criar("PETR4"), 1000, 20m);

        _custodiaRepo.Setup(r => r.ObterPorContaFilhoteAsync(It.IsAny<int>(), default))
            .ReturnsAsync(custodia);

        _cotacoes.Setup(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Ticker>>(), default))
            .ReturnsAsync(new Dictionary<string, decimal> { ["PETR4"] = 25m, ["MGLU3"] = 10m });

        var result = await CriarService().ExecutarAsync(1, 2);

        result.Detalhes[0].IrPublicado.Should().BeTrue();
        result.Detalhes[0].ValorIr.Should().Be(1000m); // 5000 * 20%

        _eventos.Verify(e =>
            e.PublicarAsync(
                "ir-venda-test",
                It.IsAny<string>(),
                It.IsAny<object>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_VendasAbaixoVinteMilReais_NaoDevePublicarIr()
    {
        var cestaAnt = CriarCesta(("PETR4", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m));
        var cestaNov = CriarCesta(("MGLU3", 20m), ("VALE3", 20m), ("ITUB4", 20m), ("BBDC4", 20m), ("ABEV3", 20m));

        _cestaRepo.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(cestaAnt);
        _cestaRepo.Setup(r => r.ObterPorIdAsync(2, default)).ReturnsAsync(cestaNov);

        var cliente = Cliente.Criar("Maria", "529.982.247-25", "maria@email.com", 300m);
        cliente.AssociarContaFilhote(ContaFilhote.Criar(0, 1));
        _clienteRepo.Setup(r => r.ObterAtivosAsync(default)).ReturnsAsync([cliente]);

        // 100 PETR4 a PM=30 → venda a 32 = R$3.200 (< 20k)
        var custodia = CustodiaFilhote.Criar(0);
        custodia.AdicionarAtivos(Ticker.Criar("PETR4"), 100, 30m);

        _custodiaRepo.Setup(r => r.ObterPorContaFilhoteAsync(It.IsAny<int>(), default))
            .ReturnsAsync(custodia);

        _cotacoes.Setup(c => c.ObterCotacoesAsync(It.IsAny<IEnumerable<Ticker>>(), default))
            .ReturnsAsync(new Dictionary<string, decimal> { ["PETR4"] = 32m, ["MGLU3"] = 10m });

        var result = await CriarService().ExecutarAsync(1, 2);

        result.Detalhes[0].IrPublicado.Should().BeFalse();
        _eventos.Verify(e =>
            e.PublicarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), default),
            Times.Never);
    }
}
