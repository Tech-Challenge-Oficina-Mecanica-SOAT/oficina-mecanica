using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.ValueObjects;
using OficinaMecanica.Infrastructure.Data;
using OficinaMecanica.Tests.Integration.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace OficinaMecanica.Tests.Integration.Controllers;

public class ClientesControllerTests : IClassFixture<OficinaMecanicaWebFactory>
{
    private readonly OficinaMecanicaWebFactory _factory;

    public ClientesControllerTests(OficinaMecanicaWebFactory factory) =>
        _factory = factory;

    private HttpClient AdminClient() =>
        _factory.CreateClient().ComToken("Admin");

    private async Task<Guid> SeedClienteAsync(string? email = null)
    {
        using var scope = _factory.Server.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cliente = new Cliente("Seed Silva", new Documento(TestDataGenerator.NextCpf()), new Telefone("(11) 91234-5678"),
            new Email(email ?? TestDataGenerator.NextEmail()));
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return cliente.Id;
    }

    [Fact]
    public async Task GetAll_ComAdmin_Retorna200()
    {
        var resp = await AdminClient().GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetAll_SemToken_Retorna401()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdExistente_Retorna200()
    {
        var id = await SeedClienteAsync();
        var resp = await AdminClient().GetAsync($"/api/clientes/{id}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().GetAsync($"/api/clientes/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetByDocumento_ComDocumentoExistente_Retorna200()
    {
        var documento = "19131243000197";
        using var scope = _factory.Server.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cliente = new Cliente("Doc Teste", new Documento(documento), new Telefone("(11) 91111-2222"),
            new Email($"doc_{Guid.NewGuid():N}@oficina.com"));
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var resp = await AdminClient().GetAsync($"/api/clientes/documento/{documento}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetByDocumento_ComDocumentoInexistente_Retorna404()
    {
        // CNPJ válido (passa na validação do VO) mas inexistente no banco
        var resp = await AdminClient().GetAsync("/api/clientes/documento/11222333000181");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComDadosValidos_Retorna201()
    {
        var dto = new CriarClienteRequest
        {
            Nome = "Novo Cliente",
            Documento = "12345678909",
            Telefone = "(11) 98765-4321",
            Email = $"novo_{Guid.NewGuid():N}@oficina.com"
        };

        var resp = await AdminClient().PostAsJsonAsync("/api/clientes", dto);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComDocumentoDuplicado_Retorna409()
    {
        var documento = "11222333000181";
        var email1 = $"dup1_{Guid.NewGuid():N}@oficina.com";
        var email2 = $"dup2_{Guid.NewGuid():N}@oficina.com";

        await AdminClient().PostAsJsonAsync("/api/clientes", new CriarClienteRequest
        {
            Nome = "Primeiro", Documento = documento,
            Telefone = "(11) 91111-0001", Email = email1
        });

        var resp = await AdminClient().PostAsJsonAsync("/api/clientes", new CriarClienteRequest
        {
            Nome = "Segundo", Documento = documento,
            Telefone = "(11) 91111-0002", Email = email2
        });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Update_ComIdExistente_Retorna200()
    {
        var id = await SeedClienteAsync();
        var dto = new AtualizarClienteRequest
        {
            Nome = "Nome Atualizado",
            Telefone = "(11) 99999-0001",
            Email = $"upd_{Guid.NewGuid():N}@oficina.com"
        };

        var resp = await AdminClient().PutAsJsonAsync($"/api/clientes/{id}", dto);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Update_ComIdInexistente_Retorna404()
    {
        var dto = new AtualizarClienteRequest
        {
            Nome = "X", Telefone = "(11) 90000-0000", Email = "x@x.com"
        };

        var resp = await AdminClient().PutAsJsonAsync($"/api/clientes/{Guid.NewGuid()}", dto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdExistente_Retorna204()
    {
        var id = await SeedClienteAsync();
        var resp = await AdminClient().DeleteAsync($"/api/clientes/{id}");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().DeleteAsync($"/api/clientes/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Ativar_ComIdExistente_Retorna204()
    {
        var id = await SeedClienteAsync();
        await AdminClient().PatchAsync($"/api/clientes/{id}/desativar", null);
        var resp = await AdminClient().PatchAsync($"/api/clientes/{id}/ativar", null);
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Ativar_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().PatchAsync($"/api/clientes/{Guid.NewGuid()}/ativar", null);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Desativar_ComIdExistente_Retorna204()
    {
        var id = await SeedClienteAsync();
        var resp = await AdminClient().PatchAsync($"/api/clientes/{id}/desativar", null);
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Desativar_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().PatchAsync($"/api/clientes/{Guid.NewGuid()}/desativar", null);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
