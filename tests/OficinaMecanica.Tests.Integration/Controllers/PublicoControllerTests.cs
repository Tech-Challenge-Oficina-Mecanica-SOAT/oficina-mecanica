using System.Net;

namespace OficinaMecanica.Tests.Integration.Controllers;

public class PublicoControllerTests : IClassFixture<OficinaMecanicaWebFactory>
{
    private readonly HttpClient _client;

    public PublicoControllerTests(OficinaMecanicaWebFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStatusOS_SemToken_ComOsInexistente_Retorna404()
    {
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"/publico/os/{id}/status");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStatusOS_SemToken_NaoRetorna401()
    {
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"/publico/os/{id}/status");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
