using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using System.Net;
using System.Net.Http.Json;

namespace OficinaMecanica.Tests.Integration.Controllers;

public class AuthControllerTests : IClassFixture<OficinaMecanicaWebFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(OficinaMecanicaWebFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string UniqueEmail(string prefix = "user") =>
        $"{prefix}_{Guid.NewGuid():N}@oficina.com";

    [Fact]
    public async Task Registrar_ComDadosValidos_Retorna201()
    {
        var response = await _client.PostAsJsonAsync("/auth/registrar",
            new RegistrarUsuarioRequest(UniqueEmail("novo"), "Senha@123"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Registrar_ComEmailDuplicado_Retorna409()
    {
        var email = UniqueEmail("dup");
        await _client.PostAsJsonAsync("/auth/registrar", new RegistrarUsuarioRequest(email, "Senha@123"));

        var response = await _client.PostAsJsonAsync("/auth/registrar",
            new RegistrarUsuarioRequest(email, "Senha@123"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_Retorna200ComToken()
    {
        var email = UniqueEmail("login");
        await _client.PostAsJsonAsync("/auth/registrar", new RegistrarUsuarioRequest(email, "Senha@123"));

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(email, "Senha@123"));
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(token?.Token);
        Assert.NotEmpty(token.Token);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_Retorna401()
    {
        var email = UniqueEmail("errado");
        await _client.PostAsJsonAsync("/auth/registrar", new RegistrarUsuarioRequest(email, "SenhaCorreta@1"));

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(email, "SenhaErrada"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComEmailInexistente_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(UniqueEmail("ghost"), "Senha@123"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
