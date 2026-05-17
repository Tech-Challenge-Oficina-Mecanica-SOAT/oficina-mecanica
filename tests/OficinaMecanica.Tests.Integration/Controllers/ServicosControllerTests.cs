using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Tests.Integration.TestHelpers;
using System.Net;
using System.Net.Http.Json;

namespace OficinaMecanica.Tests.Integration.Controllers;

public class ServicosControllerTests : IClassFixture<OficinaMecanicaWebFactory>
{
    private readonly OficinaMecanicaWebFactory _factory;

    public ServicosControllerTests(OficinaMecanicaWebFactory factory) =>
        _factory = factory;

    private HttpClient AdminClient() =>
        _factory.CreateClient().ComToken("Admin");

    private async Task<Guid> SeedServicoAsync(string? nome = null)
    {
        var createDto = new CriarServicoRequest
        {
            Nome = nome ?? $"Serviço {Guid.NewGuid():N}",
            Descricao = "Descrição automática",
            Valor = 150.00m
        };
        var resp = await AdminClient().PostAsJsonAsync("/api/servicos", createDto);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<ServicoResponse>();
        return dto!.Id;
    }

    [Fact]
    public async Task GetAll_ComAdmin_Retorna200()
    {
        var resp = await AdminClient().GetAsync("/api/servicos");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetAll_SemToken_Retorna401()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/servicos");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetAtivos_ComAdmin_Retorna200()
    {
        var resp = await AdminClient().GetAsync("/api/servicos/ativos");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdExistente_Retorna200()
    {
        var id = await SeedServicoAsync();
        var resp = await AdminClient().GetAsync($"/api/servicos/{id}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().GetAsync($"/api/servicos/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComDadosValidos_Retorna201()
    {
        var dto = new CriarServicoRequest
        {
            Nome = $"Serviço Novo {Guid.NewGuid():N}",
            Descricao = "Descrição",
            Valor = 200.00m
        };

        var resp = await AdminClient().PostAsJsonAsync("/api/servicos", dto);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComNomeDuplicado_Retorna409()
    {
        var nome = $"Serviço Dup {Guid.NewGuid():N}";
        await SeedServicoAsync(nome);

        var resp = await AdminClient().PostAsJsonAsync("/api/servicos", new CriarServicoRequest
        {
            Nome = nome, Descricao = "Outro", Valor = 100m
        });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Update_ComIdExistente_Retorna200()
    {
        var id = await SeedServicoAsync();
        var dto = new AtualizarServicoRequest
        {
            Nome = $"Atualizado {Guid.NewGuid():N}",
            Descricao = "Nova descrição",
            Valor = 300.00m
        };

        var resp = await AdminClient().PutAsJsonAsync($"/api/servicos/{id}", dto);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Update_ComIdInexistente_Retorna404()
    {
        var dto = new AtualizarServicoRequest { Nome = "X", Descricao = "Y", Valor = 10m };
        var resp = await AdminClient().PutAsJsonAsync($"/api/servicos/{Guid.NewGuid()}", dto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdExistente_Retorna204()
    {
        var id = await SeedServicoAsync();
        var resp = await AdminClient().DeleteAsync($"/api/servicos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdInexistente_Retorna204()
    {
        // O repositório usa delete idempotente (silencia IDs inexistentes), portanto retorna 204
        var resp = await AdminClient().DeleteAsync($"/api/servicos/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Ativar_ComIdExistente_Retorna204()
    {
        var id = await SeedServicoAsync();
        await AdminClient().PatchAsync($"/api/servicos/{id}/desativar", null);
        var resp = await AdminClient().PatchAsync($"/api/servicos/{id}/ativar", null);
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Ativar_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().PatchAsync($"/api/servicos/{Guid.NewGuid()}/ativar", null);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Desativar_ComIdExistente_Retorna204()
    {
        var id = await SeedServicoAsync();
        var resp = await AdminClient().PatchAsync($"/api/servicos/{id}/desativar", null);
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Desativar_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().PatchAsync($"/api/servicos/{Guid.NewGuid()}/desativar", null);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
