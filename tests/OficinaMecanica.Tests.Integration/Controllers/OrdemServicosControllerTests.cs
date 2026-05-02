using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OficinaMecanica.Application.DTOs;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Infrastructure.Data;
using OficinaMecanica.Tests.Integration.TestHelpers;
using Xunit.Abstractions;

namespace OficinaMecanica.Tests.Integration.Controllers;

public class OrdemServicosControllerTests
{
    private readonly ITestOutputHelper _output;

    public OrdemServicosControllerTests(ITestOutputHelper output) =>
        _output = output;

    private async Task<Guid> SeedOSAsync(IServiceProvider services, string email)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cliente = new Cliente("Teste", "12345678909", "(11) 99999-0000", email);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo = new Veiculo(cliente.Id, "ABC1D23", "Marca", "Modelo", 2020);
        db.Veiculos.Add(veiculo);
        await db.SaveChangesAsync();

        var os = new OrdemServico(cliente.Id, veiculo.Id, "obs")
        {
            Cliente = cliente,
            Veiculo = veiculo
        };
        db.OrdensServico.Add(os);
        await db.SaveChangesAsync();

        return os.Id;
    }

    [Fact]
    public async Task AddItens_ChamaStatusENotificacao()
    {
        var statusSpy = new OrdemServicoStatusSpy();
        var notificacaoSpy = new NotificacaoSpy();

        using var factory = new OrdemServicosWebFactory(statusSpy, notificacaoSpy);

        var client = factory.CreateClient().ComToken("Admin");
        var osId = await SeedOSAsync(factory.Server.Services, "cliente@teste.com");

        var itens = new List<CreateOrdemServicoItemDto>
        {
            new()
            {
                Tipo = "servico",
                ReferenciaId = Guid.NewGuid(),
                Descricao = "Troca de óleo",
                Quantidade = 2,
                PrecoUnitario = 100m
            }
        };

        var resp = await client.PostAsJsonAsync($"/api/ordens-servico/{osId}/itens", itens);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            _output.WriteLine(body.Length > 500 ? body[..500] : body);
        }

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        statusSpy.Calls.Should().Be(1);
        statusSpy.LastOsId.Should().Be(osId);
        statusSpy.LastAlteradoPor.Should().Be("sistema");
        notificacaoSpy.Calls.Should().Be(1);
        notificacaoSpy.LastOsId.Should().Be(osId);
        notificacaoSpy.LastEmail.Should().Be("cliente@teste.com");
        notificacaoSpy.LastTotal.Should().Be(200m);
    }

    private sealed class OrdemServicoStatusSpy : IOrdemServicoStatusService
    {
        public int Calls { get; private set; }
        public Guid? LastOsId { get; private set; }
        public string? LastAlteradoPor { get; private set; }

        public Task IniciarDiagnosticoAsync(Guid osId, string alteradoPor) => Task.CompletedTask;
        public Task MarcarAguardandoAprovacaoAsync(Guid osId, string alteradoPor)
        {
            Calls++;
            LastOsId = osId;
            LastAlteradoPor = alteradoPor;
            return Task.CompletedTask;
        }

        public Task AprovarAsync(Guid osId, string alteradoPor) => Task.CompletedTask;
        public Task RejeitarAsync(Guid osId, string alteradoPor, string motivo) => Task.CompletedTask;
        public Task NotificarConclusaoAsync(Guid osId, string alteradoPor) => Task.CompletedTask;
        public Task EntregarAsync(Guid osId, string alteradoPor) => Task.CompletedTask;
        public Task ForcarStatusAsync(Guid osId, EnumStatusOS novoStatus, string alteradoPor, string motivo) => Task.CompletedTask;
        public Task<IEnumerable<HistoricoStatusOSDto>> ObterHistoricoAsync(Guid osId) => Task.FromResult<IEnumerable<HistoricoStatusOSDto>>([]);
    }

    private sealed class NotificacaoSpy : INotificacaoService
    {
        public int Calls { get; private set; }
        public Guid? LastOsId { get; private set; }
        public string? LastEmail { get; private set; }
        public decimal? LastTotal { get; private set; }

        public Task EnviarOrcamentoAsync(Guid osId, string emailCliente, decimal totalOrcamento)
        {
            Calls++;
            LastOsId = osId;
            LastEmail = emailCliente;
            LastTotal = totalOrcamento;
            return Task.CompletedTask;
        }

        public Task EnviarConclusaoAsync(Guid osId, string emailCliente) => Task.CompletedTask;
    }

    private sealed class OrdemServicosWebFactory : OficinaMecanicaWebFactory
    {
        private readonly IOrdemServicoStatusService _statusService;
        private readonly INotificacaoService _notificacaoService;

        public OrdemServicosWebFactory(IOrdemServicoStatusService statusService, INotificacaoService notificacaoService)
        {
            _statusService = statusService;
            _notificacaoService = notificacaoService;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IOrdemServicoStatusService>();
                services.RemoveAll<INotificacaoService>();
                services.AddSingleton(_statusService);
                services.AddSingleton(_notificacaoService);
            });
        }
    }
}
