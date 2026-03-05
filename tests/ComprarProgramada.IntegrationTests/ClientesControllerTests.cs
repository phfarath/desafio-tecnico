using System.Net;
using System.Net.Http.Json;
using ComprarProgramada.Application.DTOs.Cliente;
using ComprarProgramada.IntegrationTests.Fixtures;
using FluentAssertions;

namespace ComprarProgramada.IntegrationTests;

public class ClientesControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ClientesControllerTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static AdesaoRequest RequestValida(string cpf = "529.982.247-25") =>
        new("Joao Silva", cpf, "joao@email.com", 300m);

    [Fact]
    public async Task GET_Listar_ComDados_DeveRetornar200()
    {
        await _client.PostAsJsonAsync("/api/clientes/adesao", RequestValida("480.009.130-37"));
        await _client.PostAsJsonAsync("/api/clientes/adesao", RequestValida("089.345.350-08"));

        var response = await _client.GetAsync("/api/clientes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<ClienteResumoResponse>>();
        body.Should().NotBeNull();
        body!.Count.Should().BeGreaterThanOrEqualTo(2);
        body.Should().Contain(c => c.Nome == "Joao Silva");
    }

    [Fact]
    public async Task GET_Listar_FiltrandoAtivoFalse_DeveRetornarApenasInativos()
    {
        var adesao = await _client.PostAsJsonAsync("/api/clientes/adesao", RequestValida("544.933.260-01"));
        var criado = await adesao.Content.ReadFromJsonAsync<AdesaoResponse>();

        await _client.PostAsJsonAsync($"/api/clientes/{criado!.ClienteId}/saida", new { });

        var response = await _client.GetAsync("/api/clientes?ativo=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<ClienteResumoResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(c => c.ClienteId == criado.ClienteId && !c.Ativo);
    }

    [Fact]
    public async Task POST_Adesao_DadosValidos_DeveRetornar201()
    {
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", RequestValida());

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<AdesaoResponse>();
        body!.Nome.Should().Be("Joao Silva");
        body.ValorMensal.Should().Be(300m);
        body.ValorParcela.Should().Be(100m);
        body.NumeroConta.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task POST_Adesao_CpfDuplicado_DeveRetornar409()
    {
        var cpf = "111.444.777-35";
        await _client.PostAsJsonAsync("/api/clientes/adesao", RequestValida(cpf));

        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", RequestValida(cpf));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_Adesao_ValorMensalAbaixoDoMinimo_DeveRetornar400()
    {
        var request = new AdesaoRequest("Joao", "529.982.247-25", "joao@email.com", 50m);

        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PUT_AlterarValorMensal_ClienteExistente_DeveRetornar204()
    {
        var adesao = await _client.PostAsJsonAsync("/api/clientes/adesao", RequestValida("222.444.666-93"));
        var criado = await adesao.Content.ReadFromJsonAsync<AdesaoResponse>();

        var response = await _client.PutAsJsonAsync(
            $"/api/clientes/{criado!.ClienteId}/valor-mensal",
            new AlterarValorMensalRequest(600m));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_AlterarValorMensal_ClienteInexistente_DeveRetornar404()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/clientes/99999/valor-mensal",
            new AlterarValorMensalRequest(600m));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Saida_ClienteExistente_DeveRetornar204()
    {
        var adesao = await _client.PostAsJsonAsync("/api/clientes/adesao", RequestValida("333.999.555-70"));
        var criado = await adesao.Content.ReadFromJsonAsync<AdesaoResponse>();

        var response = await _client.PostAsJsonAsync($"/api/clientes/{criado!.ClienteId}/saida", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_Saida_RotaRemovida_DeveRetornar404()
    {
        var response = await _client.DeleteAsync("/api/clientes/1");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
