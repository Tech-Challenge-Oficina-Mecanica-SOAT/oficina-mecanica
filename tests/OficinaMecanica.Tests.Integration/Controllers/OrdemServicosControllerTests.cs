using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.DTOs.Requests;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.ValueObjects;
using OficinaMecanica.Infrastructure.Data;
using OficinaMecanica.Tests.Integration.TestHelpers;
using System.Net;
using System.Net.Http.Json;
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

        var cliente = new Cliente("Teste", new Documento(TestDataGenerator.NextCpf()), new Telefone("(11) 99999-0000"), new Email(email));
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var veiculo = new Veiculo(cliente.Id, new Placa(TestDataGenerator.NextPlaca()), "Marca", "Modelo", 2020);
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
    public async Task AddItens_ChamaStatusUseCase()
    {
        var statusSpy = new MarcarAguardandoAprovacaoSpy();
        var notificacaoSpy = new NotificacaoSpy();

        using var factory = new OrdemServicosWebFactory(statusSpy, notificacaoSpy);

        var client = factory.CreateClient().ComToken("Admin");
        var osId = await SeedOSAsync(factory.Server.Services, "cliente@teste.com");

        var itens = new List<AdicionarOSItemRequest>
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
    }

    [Fact]
    public async Task AddItens_DisparaNotificacaoViaDomainEvent()
    {
        var notificacaoSpy = new NotificacaoSpy();

        using var factory = new OrdemServicosNotificacaoFactory(notificacaoSpy);

        var client = factory.CreateClient().ComToken("Admin");
        var osId = await SeedOSAsync(factory.Server.Services, "cliente@teste.com");

        var iniciar = await client.PatchAsync($"/api/ordens-servico/{osId}/iniciar-diagnostico", null);
        iniciar.IsSuccessStatusCode.Should().BeTrue();

        var itens = new List<AdicionarOSItemRequest>
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
        notificacaoSpy.Calls.Should().Be(1);
        notificacaoSpy.LastOsId.Should().Be(osId);
        notificacaoSpy.LastEmail.Should().Be("cliente@teste.com");
        notificacaoSpy.LastTotal.Should().Be(200m);
    }

    private sealed class MarcarAguardandoAprovacaoSpy : IMarcarAguardandoAprovacaoUseCase
    {
        public int Calls { get; private set; }
        public Guid? LastOsId { get; private set; }
        public string? LastAlteradoPor { get; private set; }

        public Task<Result<bool>> ExecutarAsync(MarcarAguardandoAprovacaoRequest request)
        {
            Calls++;
            LastOsId = request.OsId;
            LastAlteradoPor = request.AlteradoPor;
            return Task.FromResult(Result<bool>.Success(true));
        }
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

        public Task EnviarAprovacaoAsync(Guid osId, string emailCliente) => Task.CompletedTask;
        public Task EnviarRejeicaoAsync(Guid osId, string emailCliente, string motivo) => Task.CompletedTask;
        public Task EnviarConclusaoAsync(Guid osId, string emailCliente) => Task.CompletedTask;
        public Task EnviarEntregaAsync(Guid osId, string emailCliente) => Task.CompletedTask;
    }

    private sealed class OrdemServicosWebFactory : OficinaMecanicaWebFactory
    {
        private readonly IMarcarAguardandoAprovacaoUseCase _statusUseCase;
        private readonly INotificacaoService _notificacaoService;

        public OrdemServicosWebFactory(IMarcarAguardandoAprovacaoUseCase statusUseCase, INotificacaoService notificacaoService)
        {
            _statusUseCase = statusUseCase;
            _notificacaoService = notificacaoService;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMarcarAguardandoAprovacaoUseCase>();
                services.RemoveAll<INotificacaoService>();
                services.AddSingleton(_statusUseCase);
                services.AddSingleton(_notificacaoService);
            });
        }
    }

    private sealed class OrdemServicosNotificacaoFactory : OficinaMecanicaWebFactory
    {
        private readonly INotificacaoService _notificacaoService;

        public OrdemServicosNotificacaoFactory(INotificacaoService notificacaoService) =>
            _notificacaoService = notificacaoService;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<INotificacaoService>();
                services.AddSingleton(_notificacaoService);
            });
        }
    }
}
