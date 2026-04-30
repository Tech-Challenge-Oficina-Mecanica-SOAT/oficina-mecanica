using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }
    public DbSet<OrdemServico> OrdensServico { get; set; }
    public DbSet<PecaInsumo> PecasInsumos { get; set; }
    public DbSet<Servico> Servicos { get; set; }
    public DbSet<OrdemServicoPeca> OrdensServicoPecas { get; set; }
    public DbSet<OrdemServicoServico> OrdensServicoServicos { get; set; }
    public DbSet<HistoricoStatusOS> HistoricosStatusOS { get; set; }
    public DbSet<OrdemServicoItem> OrdensServicoItens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrdemServicoPeca>()
            .HasKey(op => new { op.OrdemServicoId, op.PecaInsumoId });

        modelBuilder.Entity<OrdemServicoServico>()
            .HasKey(os => new { os.OrdemServicoId, os.ServicoId });

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

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Documento).IsUnique();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Documento).IsRequired().HasMaxLength(14);
            entity.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CriadoEm).IsRequired();
            entity.Property(e => e.Ativo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Veiculo>(entity =>
        {
            entity.ToTable("Veiculos");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Placa).IsUnique();
            entity.Property(e => e.Placa).IsRequired().HasMaxLength(8);
            entity.Property(e => e.Marca).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Modelo).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Ano).IsRequired();
            entity.Property(e => e.CriadoEm).IsRequired();
        });

        modelBuilder.Entity<Servico>(entity =>
        {
            entity.ToTable("Servicos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Valor).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PecaInsumo>(entity =>
        {
            entity.ToTable("PecasInsumos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Preco).HasPrecision(18, 2);
            entity.Property(e => e.Quantidade).HasDefaultValue(0);
        });

        modelBuilder.Entity<OrdemServico>(entity =>
        {
            entity.ToTable("OrdensServico");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StatusOS).IsRequired();
            entity.Property(e => e.DataAbertura).IsRequired();
            entity.Property(e => e.Observacoes).HasMaxLength(2000);
            entity.Property(e => e.Total).HasPrecision(18, 2).HasDefaultValue(0);

            entity.HasMany(e => e.Historico)
                  .WithOne(h => h.OrdemServico)
                  .HasForeignKey(h => h.OrdemServicoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HistoricoStatusOS>(entity =>
        {
            entity.ToTable("HistoricoStatusOS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrdemServicoId).IsRequired();
            entity.Property(e => e.StatusNovo).IsRequired();
            entity.Property(e => e.AlteradoEm).IsRequired();
            entity.Property(e => e.AlteradoPor).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Motivo).HasMaxLength(500);
            entity.HasIndex(e => new { e.OrdemServicoId, e.AlteradoEm });
        });

        modelBuilder.Entity<OrdemServicoItem>(entity =>
        {
            entity.ToTable("OrdensServicoItens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Quantidade).IsRequired();
            entity.Property(e => e.PrecoUnitario).HasPrecision(18, 2);
            entity.Property(e => e.Tipo).IsRequired();
            entity.Ignore(e => e.Subtotal);

            entity.HasOne(e => e.OrdemServico)
                .WithMany(os => os.Itens)
                .HasForeignKey(e => e.OrdemServicoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
