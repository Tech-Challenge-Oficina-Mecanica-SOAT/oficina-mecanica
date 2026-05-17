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

public class VeiculosControllerTests : IClassFixture<OficinaMecanicaWebFactory>
{
    private readonly OficinaMecanicaWebFactory _factory;

    public VeiculosControllerTests(OficinaMecanicaWebFactory factory) =>
        _factory = factory;

    private HttpClient AdminClient() =>
        _factory.CreateClient().ComToken("Admin");

    private async Task<(Guid ClienteId, Guid VeiculoId)> SeedClienteEVeiculoAsync(string placa)
    {
        using var scope = _factory.Server.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cliente = new Cliente("Seed Veículo", new Documento("52998224725"), new Telefone("(11) 91234-5678"),
            new Email($"vei_{Guid.NewGuid():N}@oficina.com"));
        db.Clientes.Add(cliente);

        var veiculo = new Veiculo(cliente.Id, placa, "Toyota", "Corolla", 2022);
        db.Veiculos.Add(veiculo);

        await db.SaveChangesAsync();
        return (cliente.Id, veiculo.Id);
    }

    private async Task<Guid> SeedClienteAsync()
    {
        using var scope = _factory.Server.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cliente = new Cliente("Cliente Vei", new Documento("19131243000197"), new Telefone("(11) 91234-0000"),
            new Email($"cli_{Guid.NewGuid():N}@oficina.com"));
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return cliente.Id;
    }

    [Fact]
    public async Task GetAll_ComAdmin_Retorna200()
    {
        var resp = await AdminClient().GetAsync("/api/veiculos");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetAll_SemToken_Retorna401()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/veiculos");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdExistente_Retorna200()
    {
        var (_, veiculoId) = await SeedClienteEVeiculoAsync("JJJ1111");
        var resp = await AdminClient().GetAsync($"/api/veiculos/{veiculoId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdInexistente_Retorna404()
    {
        var resp = await AdminClient().GetAsync($"/api/veiculos/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetByPlaca_ComPlacaExistente_Retorna200()
    {
        var placa = "AAA1111";
        await SeedClienteEVeiculoAsync(placa);
        var resp = await AdminClient().GetAsync($"/api/veiculos/placa/{placa}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetByPlaca_ComPlacaInexistente_Retorna404()
    {
        var resp = await AdminClient().GetAsync("/api/veiculos/placa/ZZZ9999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetByClienteId_ComClienteExistente_Retorna200()
    {
        var (clienteId, _) = await SeedClienteEVeiculoAsync("BBB2222");
        var resp = await AdminClient().GetAsync($"/api/veiculos/cliente/{clienteId}");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComDadosValidos_Retorna201()
    {
        var clienteId = await SeedClienteAsync();
        var dto = new CriarVeiculoRequest
        {
            ClienteId = clienteId,
            Placa = "CCC3333",
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2023
        };

        var resp = await AdminClient().PostAsJsonAsync("/api/veiculos", dto);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComClienteInexistente_Retorna404()
    {
        var dto = new CriarVeiculoRequest
        {
            ClienteId = Guid.NewGuid(),
            Placa = "DDD4444",
            Marca = "Honda",
            Modelo = "Fit",
            Ano = 2022
        };

        var resp = await AdminClient().PostAsJsonAsync("/api/veiculos", dto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Create_ComPlacaDuplicada_Retorna409()
    {
        var clienteId = await SeedClienteAsync();
        var placa = "EEE5555";

        await AdminClient().PostAsJsonAsync("/api/veiculos", new CriarVeiculoRequest
        {
            ClienteId = clienteId, Placa = placa, Marca = "Ford", Modelo = "Ka", Ano = 2021
        });

        var resp = await AdminClient().PostAsJsonAsync("/api/veiculos", new CriarVeiculoRequest
        {
            ClienteId = clienteId, Placa = placa, Marca = "Ford", Modelo = "Focus", Ano = 2022
        });

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Update_ComIdExistente_Retorna200()
    {
        var (clienteId, veiculoId) = await SeedClienteEVeiculoAsync("FFF6666");
        var dto = new AtualizarVeiculoRequest
        {
            ClienteId = clienteId,
            Placa = "GGG7777",
            Marca = "Fiat",
            Modelo = "Uno",
            Ano = 2020
        };

        var resp = await AdminClient().PutAsJsonAsync($"/api/veiculos/{veiculoId}", dto);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Update_ComIdInexistente_Retorna404()
    {
        var dto = new AtualizarVeiculoRequest { Placa = "HHH8888", Marca = "X", Modelo = "Y", Ano = 2020 };
        var resp = await AdminClient().PutAsJsonAsync($"/api/veiculos/{Guid.NewGuid()}", dto);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdExistente_Retorna204()
    {
        var (_, veiculoId) = await SeedClienteEVeiculoAsync("III9999");
        var resp = await AdminClient().DeleteAsync($"/api/veiculos/{veiculoId}");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_ComIdInexistente_Retorna204()
    {
        // O repositório usa delete idempotente (silencia IDs inexistentes), portanto retorna 204
        var resp = await AdminClient().DeleteAsync($"/api/veiculos/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }
}
