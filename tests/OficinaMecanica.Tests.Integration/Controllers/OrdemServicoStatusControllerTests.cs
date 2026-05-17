using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.DTOs.Responses;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Infrastructure.Data;
using OficinaMecanica.Tests.Integration.TestHelpers;
using System.Net;
using System.Net.Http.Json;

namespace OficinaMecanica.Tests.Integration.Controllers;

public class OrdemServicoStatusControllerTests : IClassFixture<OficinaMecanicaWebFactory>
{
    private readonly OficinaMecanicaWebFactory _factory;

    public OrdemServicoStatusControllerTests(OficinaMecanicaWebFactory factory) =>
        _factory = factory;

    private async Task<Guid> SeedOSAsync(Action<OrdemServico>? mutate = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cliente = new Cliente("Teste", "12345678909", "(11) 99999-0000", "t@t.com");
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo = new Veiculo(cliente.Id, "ABC1D23", "Marca", "Modelo", 2020);
        db.Veiculos.Add(veiculo);
        await db.SaveChangesAsync();

        var os = new OrdemServico(cliente.Id, veiculo.Id, "obs");
        mutate?.Invoke(os);
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        return os.Id;
    }

    private HttpClient ClientCom(string role) => _factory.CreateClient().ComToken(role);

    [Fact]
    public async Task IniciarDiagnostico_DeRecebida_Retorna204()
    {
        var osId = await SeedOSAsync();
        var client = ClientCom("Mecanico");

        var resp = await client.PatchAsync($"/api/ordens-servico/{osId}/iniciar-diagnostico", null);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task IniciarDiagnostico_OSInexistente_Retorna404()
    {
        var client = ClientCom("Mecanico");
        var resp = await client.PatchAsync($"/api/ordens-servico/{Guid.NewGuid()}/iniciar-diagnostico", null);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Aprovar_DeRecebida_Retorna400()
    {
        var osId = await SeedOSAsync();
        var client = ClientCom("Cliente");

        var resp = await client.PatchAsync($"/api/ordens-servico/{osId}/aprovar", null);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FluxoCompleto_AteEntregue_FuncionaEPersisteHistorico()
    {
        var osId = await SeedOSAsync();
        var mecanico = ClientCom("Mecanico");
        var cliente = ClientCom("Cliente");
        var admin = ClientCom("Admin");

        // Recebida → EmDiagnostico
        (await mecanico.PatchAsync($"/api/ordens-servico/{osId}/iniciar-diagnostico", null)).EnsureSuccessStatusCode();

        // EmDiagnostico → AguardandoAprovacao (via service direto, simula gancho M4)
        using (var scope = _factory.Services.CreateScope())
        {
            var statusService = scope.ServiceProvider.GetRequiredService<IOrdemServicoStatusService>();
            await statusService.MarcarAguardandoAprovacaoAsync(osId, "M4-simulado");
        }

        // AguardandoAprovacao → EmExecucao
        (await cliente.PatchAsync($"/api/ordens-servico/{osId}/aprovar", null)).EnsureSuccessStatusCode();

        // EmExecucao → Finalizada (via /notificar-conclusao)
        (await mecanico.PatchAsync($"/api/ordens-servico/{osId}/notificar-conclusao", null)).EnsureSuccessStatusCode();

        // Finalizada → Entregue
        (await admin.PatchAsync($"/api/ordens-servico/{osId}/entregar", null)).EnsureSuccessStatusCode();

        // Verificar histórico completo
        var hist = await admin.GetFromJsonAsync<List<HistoricoStatusOSResponse>>($"/api/ordens-servico/{osId}/historico");
        hist.Should().NotBeNull();
        hist!.Should().HaveCount(6); // criação + 5 transições
        hist.Last().StatusNovo.Should().Be("Entregue");
    }

    [Fact]
    public async Task NotificarConclusao_DeEmExecucao_Retorna204EFinaliza()
    {
        var osId = await SeedOSAsync(os =>
        {
            os.IniciarDiagnostico("setup");
            os.EnviarParaAprovacao("setup");
            os.Aprovar("setup");
        });
        var client = ClientCom("Mecanico");

        var resp = await client.PatchAsync($"/api/ordens-servico/{osId}/notificar-conclusao", null);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var admin = ClientCom("Admin");
        var hist = await admin.GetFromJsonAsync<List<HistoricoStatusOSResponse>>($"/api/ordens-servico/{osId}/historico");
        hist!.Last().StatusNovo.Should().Be("Finalizada");
    }

    [Fact]
    public async Task Rejeitar_ComMotivo_Retorna204EAtualizaHistorico()
    {
        var osId = await SeedOSAsync(os =>
        {
            os.IniciarDiagnostico("setup");
            os.EnviarParaAprovacao("setup");
        });
        var client = ClientCom("Cliente");

        var resp = await client.PatchAsJsonAsync(
            $"/api/ordens-servico/{osId}/rejeitar",
            new RejeitarOSRequest("preço acima do orçado"));

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var admin = ClientCom("Admin");
        var hist = await admin.GetFromJsonAsync<List<HistoricoStatusOSResponse>>($"/api/ordens-servico/{osId}/historico");
        hist!.Last().StatusNovo.Should().Be("Rejeitada");
        hist!.Last().Motivo.Should().Contain("preço acima");
    }

    [Fact]
    public async Task Rejeitar_SemMotivo_Retorna400()
    {
        var osId = await SeedOSAsync(os =>
        {
            os.IniciarDiagnostico("setup");
            os.EnviarParaAprovacao("setup");
        });
        var client = ClientCom("Cliente");

        var resp = await client.PatchAsJsonAsync(
            $"/api/ordens-servico/{osId}/rejeitar",
            new RejeitarOSRequest(""));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ForcarStatus_ComRoleAdmin_PermiteOverride()
    {
        var osId = await SeedOSAsync();
        var admin = ClientCom("Admin");

        var resp = await admin.PatchAsJsonAsync(
            $"/api/ordens-servico/{osId}/status",
            new TransicaoStatusOSRequest(EnumStatusOS.Finalizada, "correção pós-incidente"));

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ForcarStatus_ComRoleNaoAdmin_Retorna403()
    {
        var osId = await SeedOSAsync();
        var mecanico = ClientCom("Mecanico");

        var resp = await mecanico.PatchAsJsonAsync(
            $"/api/ordens-servico/{osId}/status",
            new TransicaoStatusOSRequest(EnumStatusOS.Finalizada, "tentativa indevida"));

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoints_SemToken_Retornam401()
    {
        var osId = await SeedOSAsync();
        var anon = _factory.CreateClient();

        var resp = await anon.PatchAsync($"/api/ordens-servico/{osId}/iniciar-diagnostico", null);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
