using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.API.Configuration;
using OficinaMecanica.API.OpenApi;
using OficinaMecanica.Application;
using OficinaMecanica.Application.Configuration;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Auth;
using OficinaMecanica.Infrastructure.Data;
using OficinaMecanica.Infrastructure.Logging;
using OficinaMecanica.Infrastructure.Notifications;
using OficinaMecanica.Infrastructure.Repositories;
using OficinaMecanica.Infrastructure.Security;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Banco de Dados Postgrees
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Settings
builder.Services.AddSingleton<IJwtSettings, JwtSettings>();
builder.Services.AddSingleton<IPasswordSettings, PasswordSettings>();

// DI - Cliente
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// DI - Servico
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IServicoService, ServicoService>();

// Registrar Peca
builder.Services.AddScoped<IPecaInsumoRepository, PecaInsumoRepository>();
builder.Services.AddScoped<IPecaService, PecaService>();

// DI - Veiculo
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

// DI - Segurança
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();

// DI - OrdemServico
builder.Services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
builder.Services.AddScoped<IOrdemServicoService, OrdemServicoService>();

// DI - M3: Status e Histórico
builder.Services.AddScoped<IHistoricoStatusOSRepository, HistoricoStatusOSRepository>();
builder.Services.AddScoped<IOrdemServicoStatusService, OrdemServicoStatusService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();

// DI - Logging
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
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

var app = builder.Build();

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
}

app.Run();

public partial class Program { }