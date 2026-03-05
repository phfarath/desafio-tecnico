using System.Net;
using System.Net.Http.Json;
using ComprarProgramada.Application.DTOs.Cesta;
using ComprarProgramada.IntegrationTests.Fixtures;
using FluentAssertions;

namespace ComprarProgramada.IntegrationTests;

public class CestaControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CestaControllerTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CriarCestaRequest RequestValida(string nome = "Top Five Jan") =>
        new(nome,
        [
            new("PETR4", 25m), new("VALE3", 25m), new("ITUB4", 20m),
            new("BBDC4", 15m), new("ABEV3", 15m)
        ]);

    [Fact]
    public async Task POST_CriarCesta_DadosValidos_DeveRetornar201()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/cesta", RequestValida());

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CestaResponse>();
        body!.Nome.Should().Be("Top Five Jan");
        body.Ativa.Should().BeTrue();
        body.Itens.Should().HaveCount(5);
    }

    [Fact]
    public async Task POST_CriarCesta_MenosDeCincoAtivos_DeveRetornar400()
    {
        var request = new CriarCestaRequest("Invalida",
        [
            new("PETR4", 60m), new("VALE3", 40m)
        ]);

        var response = await _client.PostAsJsonAsync("/api/admin/cesta", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_CriarCesta_SomaDiferenteDe100_DeveRetornar400()
    {
        var request = new CriarCestaRequest("Invalida",
        [
            new("PETR4", 20m), new("VALE3", 20m), new("ITUB4", 20m),
            new("BBDC4", 20m), new("ABEV3", 10m)
        ]);

        var response = await _client.PostAsJsonAsync("/api/admin/cesta", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_CestaAtual_SemCesta_DeveRetornar400()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/cesta/atual");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_CestaAtual_ComCestaCriada_DeveRetornar200()
    {
        await _client.PostAsJsonAsync("/api/admin/cesta", RequestValida("Top Five Teste"));

        var response = await _client.GetAsync("/api/admin/cesta/atual");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<CestaResponse>();
        body!.Ativa.Should().BeTrue();
    }

    [Fact]
    public async Task GET_CestaAtiva_RotaRemovida_DeveRetornar404()
    {
        var response = await _client.GetAsync("/api/admin/cesta/ativa");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
