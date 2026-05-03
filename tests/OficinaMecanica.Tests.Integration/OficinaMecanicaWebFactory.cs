using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Tests.Integration;

public class OficinaMecanicaWebFactory : WebApplicationFactory<Program>
{
    // Um nome fixo por instância de factory — todos os requests da mesma factory compartilham o mesmo banco InMemory
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();

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
                ["ConnectionStrings:DefaultConnection"] = ""
            });
        });

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                         || d.ServiceType == typeof(ApplicationDbContext)
                         || (d.ServiceType.IsGenericType
                             && d.ServiceType.GetGenericTypeDefinition().Name.StartsWith("IDbContextOptionsConfiguration")
                             && d.ServiceType.GenericTypeArguments[0] == typeof(ApplicationDbContext)))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            var dbName = _dbName;
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }
}
