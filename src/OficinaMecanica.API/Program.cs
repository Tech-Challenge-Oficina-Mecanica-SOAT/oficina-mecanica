using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.API;
using OficinaMecanica.Application;
using OficinaMecanica.Application.Interfaces;
using OficinaMecanica.Application.Services;
using OficinaMecanica.Domain.Interfaces;
using OficinaMecanica.Infrastructure.Data;
using OficinaMecanica.Infrastructure.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar Banco de Dados Postgrees
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI - Cliente
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// DI - Veiculo
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

// DI - Segurança
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IJwtService, JwtService>();

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
    options.AddPolicy(Policies.Admin,    p => p.RequireRole(Policies.Admin));
    options.AddPolicy(Policies.Mecanico, p => p.RequireRole(Policies.Mecanico));
    options.AddPolicy(Policies.Cliente,  p => p.RequireRole(Policies.Cliente));
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("API Oficina Mecanica - Tech Challenge FIAP SOAT");
    });

    app.UseScalar(options =>
    {
        options.Title = "Oficina API";
        options.Theme = OficinaMecanica.API.ScalarTheme.Light;
        options.DefaultHttpClient = new ScalarHttpClientOptions
        {
            BaseUrl = "http://localhost:5000"
        };
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
