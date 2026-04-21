using Microsoft.EntityFrameworkCore;
using OficinaMecanica.API;
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

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("API Oficina Mecanica - Tech Challenge FIAP SOAT");
    });


    // Configura o Scalar UI
    app.UseScalar(options =>
    {
        options.Title = "Oficina API";
        options.Theme = OficinaMecanica.API.ScalarTheme.Light; // ou Light
        options.DefaultHttpClient = new ScalarHttpClientOptions
        {
            BaseUrl = "http://localhost:5000" // URL da sua API
        };
    });

}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
