using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Tests.Integration.TestHelpers;
using System.Net;
using System.Net.Http.Json;

namespace OficinaMecanica.Tests.Integration.Controllers;

public class PecasControllerTests : IClassFixture<OficinaMecanicaWebFactory>
{
    private readonly OficinaMecanicaWebFactory _factory;

    public PecasControllerTests(OficinaMecanicaWebFactory factory) =>
        _factory = factory;

    private HttpClient AdminClient() =>
        _factory.CreateClient().ComToken("Admin");

    private async Task<Guid> SeedPecaAsync(string? codigo = null, int estoque = 10)
    {
        var dto = new CriarPecaRequest
        {
            Nome = $"Peça {Guid.NewGuid():N}",
            Codigo = codigo ?? $"COD-{Guid.NewGuid():N}"[..12],
            Descricao = "Descrição automática",
            PrecoUnitario = 49.90m,
            Estoque = estoque
        };
        var resp = await AdminClient().PostAsJsonAsync("/api/pecas", dto);
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<PecaResponse>();
        return result!.Id;
    }

    [Fact]
    public async Task GetAll_ComAdmin_Retorna200()
    {
        var resp = await AdminClient().GetAsync("/api/pecas");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetAll_SemToken_Retorna401()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/pecas");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdExistente_Retorna200()
    {
        var id = await SeedPecaAsync();
        var resp = await AdminClient().GetAsync($"/api/pecas/{id}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().GetAsync($"/api/pecas/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetByCodigo_ComCodigoExistente_Retorna200()
    {
        var codigo = $"TSTCOD{Guid.NewGuid():N}"[..10];
        await SeedPecaAsync(codigo);
        var resp = await AdminClient().GetAsync($"/api/pecas/codigo/{codigo}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetByCodigo_ComCodigoInexistente_Retorna404()
    {
        var resp = await AdminClient().GetAsync("/api/pecas/codigo/CODINEXISTENTE999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetEstoqueBaixo_ComLimite_Retorna200()
    {
        await SeedPecaAsync(estoque: 2);
        var resp = await AdminClient().GetAsync("/api/pecas/estoque-baixo?limite=5");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetEstoque_ComIdExistente_Retorna200()
    {
        var id = await SeedPecaAsync(estoque: 7);
        var resp = await AdminClient().GetAsync($"/api/pecas/{id}/estoque");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetEstoque_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().GetAsync($"/api/pecas/{Guid.NewGuid()}/estoque");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComDadosValidos_Retorna201()
    {
        var dto = new CriarPecaRequest
        {
            Nome = "Filtro de Ar",
            Codigo = $"FAR-{Guid.NewGuid():N}"[..10],
            Descricao = "Filtro de ar esportivo",
            PrecoUnitario = 89.90m,
            Estoque = 20
        };

        var resp = await AdminClient().PostAsJsonAsync("/api/pecas", dto);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComCodigoDuplicado_Retorna409()
    {
        var codigo = $"DUP-{Guid.NewGuid():N}"[..10];
        await SeedPecaAsync(codigo);

        var resp = await AdminClient().PostAsJsonAsync("/api/pecas", new CriarPecaRequest
        {
            Nome = "Outra Peça", Codigo = codigo,
            Descricao = "Desc", PrecoUnitario = 10m, Estoque = 1
        });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Update_ComIdExistente_Retorna200()
    {
        var id = await SeedPecaAsync();
        var dto = new AtualizarPecaRequest
        {
            Nome = "Nome Atualizado",
            Descricao = "Nova descrição",
            PrecoUnitario = 99.00m,
            Estoque = 15
        };

        var resp = await AdminClient().PutAsJsonAsync($"/api/pecas/{id}", dto);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Update_ComIdInexistente_Retorna404()
    {
        var dto = new AtualizarPecaRequest { Nome = "X", Descricao = "Y", PrecoUnitario = 1m, Estoque = 0 };
        var resp = await AdminClient().PutAsJsonAsync($"/api/pecas/{Guid.NewGuid()}", dto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateEstoque_Incrementar_Retorna200()
    {
        var id = await SeedPecaAsync(estoque: 5);
        var dto = new AtualizarEstoqueRequest { Quantidade = 10, TipoOperacao = "incrementar" };

        var resp = await AdminClient().PatchAsJsonAsync($"/api/pecas/{id}/estoque", dto);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateEstoque_Decrementar_Retorna200()
    {
        var id = await SeedPecaAsync(estoque: 10);
        var dto = new AtualizarEstoqueRequest { Quantidade = 3, TipoOperacao = "decrementar" };

        var resp = await AdminClient().PatchAsJsonAsync($"/api/pecas/{id}/estoque", dto);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateEstoque_ComIdInexistente_Retorna404()
    {
        var dto = new AtualizarEstoqueRequest { Quantidade = 1, TipoOperacao = "incrementar" };
        var resp = await AdminClient().PatchAsJsonAsync($"/api/pecas/{Guid.NewGuid()}/estoque", dto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdExistente_Retorna204()
    {
        var id = await SeedPecaAsync();
        var resp = await AdminClient().DeleteAsync($"/api/pecas/{id}");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().DeleteAsync($"/api/pecas/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
