using System.Net;
using ComprarProgramada.IntegrationTests.Fixtures;
using FluentAssertions;

namespace ComprarProgramada.IntegrationTests;

public class MotorControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public MotorControllerTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_ExecutarCompra_SemPreRequisitos_DeveRetornar400()
    {
        var response = await _client.PostAsync("/api/motor/executar-compra", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Executar_RotaRemovida_DeveRetornar404()
    {
        var response = await _client.PostAsync("/api/motor/executar", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
