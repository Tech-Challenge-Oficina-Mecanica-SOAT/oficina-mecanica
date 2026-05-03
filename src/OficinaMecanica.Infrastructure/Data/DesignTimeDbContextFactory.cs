using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OficinaMecanica.Infrastructure.Data;

namespace OficinaMecanica.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var conn = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
               ?? "Host=localhost;Database=OficinaDB;Username=postgres;Password=SuaSenha";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseNpgsql(conn);
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
