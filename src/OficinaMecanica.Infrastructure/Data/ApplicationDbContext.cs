using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options){ }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }
    public DbSet<OrdemServico> OrdensServico { get; set; }
    public DbSet<PecaInsumo> PecasInsumos { get; set; }
    public DbSet<Servico> Servicos { get; set; }
    public DbSet<OrdemServicoPeca> OrdensServicoPecas { get; set; }
    public DbSet<OrdemServicoServico> OrdensServicoServicos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Chaves compostas para tabelas de junção
        modelBuilder.Entity<OrdemServicoPeca>()
            .HasKey(op => new { op.OrdemServicoId, op.PecaInsumoId });

        modelBuilder.Entity<OrdemServicoServico>()
            .HasKey(os => new { os.OrdemServicoId, os.ServicoId });

        // Relacionamentos
        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Veiculos)
            .WithOne(v => v.Cliente)
            .HasForeignKey(v => v.ClienteId);

        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.OrdensServico)
            .WithOne(os => os.Cliente)
            .HasForeignKey(os => os.ClienteId);

        modelBuilder.Entity<Veiculo>()
            .HasMany(v => v.OrdensServico)
            .WithOne(os => os.Veiculo)
            .HasForeignKey(os => os.VeiculoId);

    }
}
