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

    public async Task InitializeAsync() => await _db.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
