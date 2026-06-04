using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace OficinaMecanica.Tests.Integration;

public class OficinaMecanicaWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private Task? _startTask;

    public async Task InitializeAsync()
    {
        _startTask = _db.StartAsync();
        await _startTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // If InitializeAsync was not called (manual factory creation), start the container now
        if (_startTask is null)
        {
            _startTask = _db.StartAsync();
            _startTask.GetAwaiter().GetResult();
        }
        else
        {
            _startTask.GetAwaiter().GetResult();
        }

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "mecanica-jwt-secret-key-minimo-32-chars!!",
                ["Jwt:Issuer"] = "mecanica-api",
                ["Jwt:Audience"] = "mecanica-cliente",
                ["Jwt:ExpiracaoMinutos"] = "5",
                ["Seguranca:PasswordKey"] = "K7mP2nQx9vR4wL8sY1tZ6uA3cE5gJ0hF",
                ["ConnectionStrings:DefaultConnection"] = _db.GetConnectionString()
            });
        });
    }
}
