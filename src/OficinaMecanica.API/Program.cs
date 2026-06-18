using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.API.Configuration;
using OficinaMecanica.API.OpenApi;
using OficinaMecanica.Application;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Application.EventHandlers;
using OficinaMecanica.Domain.Events;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Mappers;
using OficinaMecanica.Application.UseCases.Auth.AutenticarUsuario;
using OficinaMecanica.Application.UseCases.Auth.RegistrarUsuario;
using OficinaMecanica.Application.UseCases.Cliente.AtivarCliente;
using OficinaMecanica.Application.UseCases.Cliente.AtualizarCliente;
using OficinaMecanica.Application.UseCases.Cliente.ConsultarCliente;
using OficinaMecanica.Application.UseCases.Cliente.ConsultarClientePorDocumento;
using OficinaMecanica.Application.UseCases.Cliente.CriarCliente;
using OficinaMecanica.Application.UseCases.Cliente.DesativarCliente;
using OficinaMecanica.Application.UseCases.Cliente.ListarClientes;
using OficinaMecanica.Application.UseCases.Cliente.RemoverCliente;
using OficinaMecanica.Application.UseCases.OrdemServico.AbrirOrdemServico;
using OficinaMecanica.Application.UseCases.OrdemServico.AdicionarItensOS;
using OficinaMecanica.Application.UseCases.OrdemServico.ConsultarOrdemServico;
using OficinaMecanica.Application.UseCases.OrdemServico.ListarOrdensServico;
using OficinaMecanica.Application.UseCases.OrdemServico.ObterTempoMedioExecucao;
using OficinaMecanica.Application.UseCases.OrdemServico.RemoverItemOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.EntregarOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.ForcarStatusOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.IniciarDiagnostico;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.MarcarAguardandoAprovacao;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.NotificarConclusao;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.ObterHistoricoOS;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.RejeitarOS;
using OficinaMecanica.Application.UseCases.Peca.AtualizarEstoque;
using OficinaMecanica.Application.UseCases.Peca.AtualizarPeca;
using OficinaMecanica.Application.UseCases.Peca.ConsultarPeca;
using OficinaMecanica.Application.UseCases.Peca.CriarPeca;
using OficinaMecanica.Application.UseCases.Peca.ListarPecas;
using OficinaMecanica.Application.UseCases.Peca.ListarPecasEstoqueBaixo;
using OficinaMecanica.Application.UseCases.Peca.ListarPecasPorNome;
using OficinaMecanica.Application.UseCases.Peca.ObterEstoque;
using OficinaMecanica.Application.UseCases.Peca.RemoverPeca;
using OficinaMecanica.Application.UseCases.Servico.AtivarServico;
using OficinaMecanica.Application.UseCases.Servico.AtualizarServico;
using OficinaMecanica.Application.UseCases.Servico.ConsultarServico;
using OficinaMecanica.Application.UseCases.Servico.CriarServico;
using OficinaMecanica.Application.UseCases.Servico.DesativarServico;
using OficinaMecanica.Application.UseCases.Servico.ListarServicos;
using OficinaMecanica.Application.UseCases.Servico.ListarServicosAtivos;
using OficinaMecanica.Application.UseCases.Servico.ListarServicosPorNome;
using OficinaMecanica.Application.UseCases.Servico.RemoverServico;
using OficinaMecanica.Application.UseCases.Veiculo.AtualizarVeiculo;
using OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculo;
using OficinaMecanica.Application.UseCases.Veiculo.ConsultarVeiculoPorPlaca;
using OficinaMecanica.Application.UseCases.Veiculo.CriarVeiculo;
using OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculos;
using OficinaMecanica.Application.UseCases.Veiculo.ListarVeiculosPorCliente;
using OficinaMecanica.Application.UseCases.Veiculo.RemoverVeiculo;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Auth;
using OficinaMecanica.Infrastructure.Data;
using OficinaMecanica.Infrastructure.Events;
using OficinaMecanica.Infrastructure.Logging;
using OficinaMecanica.Infrastructure.Notifications;
using OficinaMecanica.Infrastructure.Repositories;
using OficinaMecanica.Infrastructure.Security;
using Scalar.AspNetCore;
using System.Text;
using OficinaMecanica.Infrastructure.Configuration;
using OficinaMecanica.Application.UseCases.OrdemServicoStatus.AprovarOrcamentoPorEmail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Banco de Dados Postgrees
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Settings
var jwtSettings = new JwtSettings(builder.Configuration);
builder.Services.AddSingleton<IJwtSettings>(jwtSettings);
builder.Services.AddSingleton<IPasswordSettings, PasswordSettings>();

// DI - Repositories
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IPecaInsumoRepository, PecaInsumoRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
builder.Services.AddScoped<IHistoricoStatusOSRepository, HistoricoStatusOSRepository>();

// DI - Segurança
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

// DI - Notificacoes
if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddScoped<INotificacaoService, EmailNotificacaoServiceFake>();
}
else
{
    builder.Services.AddScoped<INotificacaoService, EmailNotificacaoService>();
}
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));
builder.Services.AddScoped<IAprovarOrcamentoPorEmailUseCase, AprovarOrcamentoPorEmailUseCase>();

// DI - Domain Events
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IEventHandler<OrcamentoEnviadoEvent>, NotificarOrcamentoHandler>();
builder.Services.AddScoped<IEventHandler<OrdemAprovadaEvent>, NotificarAprovacaoHandler>();
builder.Services.AddScoped<IEventHandler<OrdemRejeitadaEvent>, NotificarRejeicaoHandler>();
builder.Services.AddScoped<IEventHandler<OrdemConcluidaEvent>, NotificarConclusaoHandler>();
builder.Services.AddScoped<IEventHandler<OrdemEntregueEvent>, NotificarEntregaHandler>();
builder.Services.AddScoped<IEventHandler<OrdemCriadaEvent>, NotificarCriacaoHandler>();
builder.Services.AddScoped<IEventHandler<DiagnosticoIniciadoEvent>, NotificarDiagnosticoHandler>();

// DI - Logging
builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

// DI - Mappers
builder.Services.AddSingleton<OrdemServicoMapper>();
builder.Services.AddSingleton<ClienteMapper>();
builder.Services.AddSingleton<VeiculoMapper>();
builder.Services.AddSingleton<PecaMapper>();
builder.Services.AddSingleton<ServicoMapper>();
builder.Services.AddSingleton<HistoricoStatusOSMapper>();

// DI - Auth Use Cases
builder.Services.AddScoped<IAutenticarUsuarioUseCase, AutenticarUsuarioUseCase>();
builder.Services.AddScoped<IRegistrarUsuarioUseCase, RegistrarUsuarioUseCase>();

// DI - Cliente Use Cases
builder.Services.AddScoped<ICriarClienteUseCase, CriarClienteUseCase>();
builder.Services.AddScoped<IAtualizarClienteUseCase, AtualizarClienteUseCase>();
builder.Services.AddScoped<IConsultarClienteUseCase, ConsultarClienteUseCase>();
builder.Services.AddScoped<IConsultarClientePorDocumentoUseCase, ConsultarClientePorDocumentoUseCase>();
builder.Services.AddScoped<IListarClientesUseCase, ListarClientesUseCase>();
builder.Services.AddScoped<IRemoverClienteUseCase, RemoverClienteUseCase>();
builder.Services.AddScoped<IAtivarClienteUseCase, AtivarClienteUseCase>();
builder.Services.AddScoped<IDesativarClienteUseCase, DesativarClienteUseCase>();

// DI - Veiculo Use Cases
builder.Services.AddScoped<ICriarVeiculoUseCase, CriarVeiculoUseCase>();
builder.Services.AddScoped<IAtualizarVeiculoUseCase, AtualizarVeiculoUseCase>();
builder.Services.AddScoped<IConsultarVeiculoUseCase, ConsultarVeiculoUseCase>();
builder.Services.AddScoped<IConsultarVeiculoPorPlacaUseCase, ConsultarVeiculoPorPlacaUseCase>();
builder.Services.AddScoped<IListarVeiculosUseCase, ListarVeiculosUseCase>();
builder.Services.AddScoped<IListarVeiculosPorClienteUseCase, ListarVeiculosPorClienteUseCase>();
builder.Services.AddScoped<IRemoverVeiculoUseCase, RemoverVeiculoUseCase>();

// DI - Peca Use Cases
builder.Services.AddScoped<ICriarPecaUseCase, CriarPecaUseCase>();
builder.Services.AddScoped<IAtualizarPecaUseCase, AtualizarPecaUseCase>();
builder.Services.AddScoped<IAtualizarEstoqueUseCase, AtualizarEstoqueUseCase>();
builder.Services.AddScoped<IConsultarPecaUseCase, ConsultarPecaUseCase>();
builder.Services.AddScoped<IListarPecasUseCase, ListarPecasUseCase>();
builder.Services.AddScoped<IListarPecasEstoqueBaixoUseCase, ListarPecasEstoqueBaixoUseCase>();
builder.Services.AddScoped<IListarPecasPorNomeUseCase, ListarPecasPorNomeUseCase>();
builder.Services.AddScoped<IObterEstoqueUseCase, ObterEstoqueUseCase>();
builder.Services.AddScoped<IRemoverPecaUseCase, RemoverPecaUseCase>();

// DI - Servico Use Cases
builder.Services.AddScoped<ICriarServicoUseCase, CriarServicoUseCase>();
builder.Services.AddScoped<IAtualizarServicoUseCase, AtualizarServicoUseCase>();
builder.Services.AddScoped<IConsultarServicoUseCase, ConsultarServicoUseCase>();
builder.Services.AddScoped<IListarServicosUseCase, ListarServicosUseCase>();
builder.Services.AddScoped<IListarServicosAtivosUseCase, ListarServicosAtivosUseCase>();
builder.Services.AddScoped<IListarServicosPorNomeUseCase, ListarServicosPorNomeUseCase>();
builder.Services.AddScoped<IRemoverServicoUseCase, RemoverServicoUseCase>();
builder.Services.AddScoped<IAtivarServicoUseCase, AtivarServicoUseCase>();
builder.Services.AddScoped<IDesativarServicoUseCase, DesativarServicoUseCase>();

// DI - OrdemServico Use Cases
builder.Services.AddScoped<IAbrirOrdemServicoUseCase, AbrirOrdemServicoUseCase>();
builder.Services.AddScoped<IConsultarOrdemServicoUseCase, ConsultarOrdemServicoUseCase>();
builder.Services.AddScoped<IListarOrdensServicoUseCase, ListarOrdensServicoUseCase>();
builder.Services.AddScoped<IAdicionarItensOSUseCase, AdicionarItensOSUseCase>();
builder.Services.AddScoped<IRemoverItemOSUseCase, RemoverItemOSUseCase>();
builder.Services.AddScoped<IObterTempoMedioExecucaoUseCase, ObterTempoMedioExecucaoUseCase>();

// DI - OrdemServicoStatus Use Cases
builder.Services.AddScoped<IIniciarDiagnosticoUseCase, IniciarDiagnosticoUseCase>();
builder.Services.AddScoped<IAprovarOSUseCase, AprovarOSUseCase>();
builder.Services.AddScoped<IRejeitarOSUseCase, RejeitarOSUseCase>();
builder.Services.AddScoped<INotificarConclusaoUseCase, NotificarConclusaoUseCase>();
builder.Services.AddScoped<IEntregarOSUseCase, EntregarOSUseCase>();
builder.Services.AddScoped<IForcarStatusOSUseCase, ForcarStatusOSUseCase>();
builder.Services.AddScoped<IObterHistoricoOSUseCase, ObterHistoricoOSUseCase>();
builder.Services.AddScoped<IMarcarAguardandoAprovacaoUseCase, MarcarAguardandoAprovacaoUseCase>();

// Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.Admin, p => p.RequireRole(Policies.Admin));
    options.AddPolicy(Policies.Mecanico, p => p.RequireRole(Policies.Mecanico));
    options.AddPolicy(Policies.Cliente, p => p.RequireRole(Policies.Cliente));
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<JwtBearerDocumentTransformer>();
    options.AddSchemaTransformer<ExampleSchemaTransformer>();
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.WithTitle("API Oficina Mecanica - Tech Challenge FIAP SOAT");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (context.Database.IsRelational())
        context.Database.Migrate();

    if (!context.Set<OficinaMecanica.Domain.Entities.Usuario>().Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var adminHash = hasher.Hash("Senha@123");
        context.Set<OficinaMecanica.Domain.Entities.Usuario>().Add(
            new OficinaMecanica.Domain.Entities.Usuario("admin@oficina.com", adminHash, OficinaMecanica.Domain.Enums.Perfil.Admin));
        context.SaveChanges();
    }
}

app.Run();

public partial class Program { }
