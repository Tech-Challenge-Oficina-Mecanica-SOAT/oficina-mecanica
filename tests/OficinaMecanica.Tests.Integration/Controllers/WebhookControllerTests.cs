using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.ValueObjects;
using OficinaMecanica.Infrastructure.Data;
using OficinaMecanica.Tests.Integration.TestHelpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace OficinaMecanica.Tests.Integration.Controllers;

public class WebhookControllerTests : IClassFixture<OficinaMecanicaWebFactory>
{
    private readonly HttpClient _client;
    private readonly OficinaMecanicaWebFactory _factory;

    public WebhookControllerTests(OficinaMecanicaWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient().ComToken("Admin");
    }

    private async Task<(Guid osId, string token)> CriarOSComTokenAsync()
    {
        // Criar cliente
        var clienteRequest = new CriarClienteRequest
        {
            Nome = "Cliente Teste",
            Email = "cliente@teste.com",
            Documento = TestDataGenerator.NextCpf(),
            Telefone = "(11) 99999-0000"
        };
        
        var clienteResponse = await _client.PostAsJsonAsync("/api/clientes", clienteRequest);
        var clienteJson = await clienteResponse.Content.ReadAsStringAsync();
        var cliente = JsonSerializer.Deserialize<JsonElement>(clienteJson);
        var clienteId = cliente.GetProperty("id").GetGuid();

        // Criar veículo
        var veiculoRequest = new CriarVeiculoRequest
        {
            ClienteId = clienteId,
            Placa = TestDataGenerator.NextPlaca(),
            Marca = "Marca Teste",
            Modelo = "Modelo Teste",
            Ano = 2020
        };
        
        var veiculoResponse = await _client.PostAsJsonAsync("/api/veiculos", veiculoRequest);
        var veiculoJson = await veiculoResponse.Content.ReadAsStringAsync();
        var veiculo = JsonSerializer.Deserialize<JsonElement>(veiculoJson);
        var veiculoId = veiculo.GetProperty("id").GetGuid();

        // Criar OS
        var osRequest = new AbrirOrdemServicoRequest
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Observacoes = "Teste"
        };
        
        var osResponse = await _client.PostAsJsonAsync("/api/ordens-servico", osRequest);
        var osJson = await osResponse.Content.ReadAsStringAsync();
        var osObj = JsonSerializer.Deserialize<JsonElement>(osJson);
        var osId = osObj.GetProperty("id").GetGuid();

        // Iniciar diagnóstico
        await _client.PatchAsync($"/api/ordens-servico/{osId}/iniciar-diagnostico", null);

        // Adicionar itens
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var servico = new Servico("Servico Teste", "Descricao", 100m);
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        var itens = new List<AdicionarOSItemRequest>
        {
            new() { 
                Tipo = "servico", 
                ReferenciaId = servico.Id, 
                Quantidade = 1, 
                Descricao = servico.Nome, 
                PrecoUnitario = servico.Valor 
            }
        };
        await _client.PostAsJsonAsync($"/api/ordens-servico/{osId}/itens", itens);

        // Enviar para aprovação
        await _client.PatchAsync($"/api/ordens-servico/{osId}/enviar-para-aprovacao", null);

        // Gerar token manualmente (já que o Fake não gera)
        var osEntity = await db.OrdensServico.FindAsync(osId);
        if (string.IsNullOrEmpty(osEntity!.TokenAprovacao))
        {
            var token = Guid.NewGuid().ToString("N").Substring(0, 32);
            osEntity.TokenAprovacao = token;
            osEntity.TokenUsado = false;
            await db.SaveChangesAsync();
        }

        return (osId, osEntity.TokenAprovacao!);
    }

    [Fact]
    public async Task AprovarOrcamento_ComTokenValido_RetornaPaginaAprovacao()
    {
        var (osId, token) = await CriarOSComTokenAsync();

        var url = $"/api/webhooks/ordens-servico/aprovar/{token}?aprovado=true";
        var response = await _client.GetAsync(url);

        Console.WriteLine($"=== DEBUG ===");
        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine($"URL: {url}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Orçamento Aprovado");
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var osAtualizada = await db.OrdensServico.FindAsync(osId);
        osAtualizada!.TokenUsado.Should().BeTrue();
        osAtualizada.StatusOS.Should().Be(EnumStatusOS.EmExecucao);
    }

    [Fact]
    public async Task AprovarOrcamento_ComTokenInvalido_RetornaPaginaErro()
    {
        var response = await _client.GetAsync("/api/webhooks/ordens-servico/aprovar/token-invalido?aprovado=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Erro");
        content.Should().Contain("Token inválido");
    }

    [Fact]
    public async Task AprovarOrcamento_ComTokenJaUsado_RetornaPaginaErro()
    {
        var (_, token) = await CriarOSComTokenAsync();
        
        await _client.GetAsync($"/api/webhooks/ordens-servico/aprovar/{token}?aprovado=true");
        
        var response = await _client.GetAsync($"/api/webhooks/ordens-servico/aprovar/{token}?aprovado=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Erro");
        content.Should().Contain("Link já foi utilizado");
    }

    [Fact]
    public async Task RecusarOrcamento_ComTokenValido_RetornaPaginaRecusado()
    {
        var (osId, token) = await CriarOSComTokenAsync();

        var response = await _client.GetAsync($"/api/webhooks/ordens-servico/aprovar/{token}?aprovado=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Orçamento Recusado");
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var osAtualizada = await db.OrdensServico.FindAsync(osId);
        osAtualizada!.TokenUsado.Should().BeTrue();
        osAtualizada.StatusOS.Should().Be(EnumStatusOS.Rejeitada);
    }
}