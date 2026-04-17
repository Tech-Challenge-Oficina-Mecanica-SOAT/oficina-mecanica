using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options){ }

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


        //// Configuração da entidade Cliente
        //modelBuilder.Entity<Cliente>(entity =>
        //{
        //    entity.ToTable("Clientes");
        //    entity.HasKey(e => e.Id);
        //    entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        //    entity.Property(e => e.Documento).IsRequired().HasMaxLength(14);
        //    entity.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
        //    entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
        //    entity.Property(e => e.CriadoEm).IsRequired();
        //    entity.Property(e => e.Ativo).IsRequired().HasDefaultValue(true);

        //    entity.HasIndex(e => e.Documento).IsUnique();
        //});

        //// Configuração da entidade Veiculo
        //modelBuilder.Entity<Veiculo>(entity =>
        //{
        //    entity.ToTable("Veiculos");
        //    entity.HasKey(e => e.Id);
        //    entity.Property(e => e.ClienteId).IsRequired();
        //    entity.Property(e => e.Placa).IsRequired().HasMaxLength(8);
        //    entity.Property(e => e.Marca).IsRequired().HasMaxLength(100);
        //    entity.Property(e => e.Modelo).IsRequired().HasMaxLength(100);
        //    entity.Property(e => e.Ano).IsRequired();
        //    entity.Property(e => e.CriadoEm).IsRequired();

        //    entity.HasIndex(e => e.Placa).IsUnique();

        //    entity.HasOne(e => e.Cliente)
        //        .WithMany()
        //        .HasForeignKey(e => e.ClienteId)
        //        .OnDelete(DeleteBehavior.Restrict);
        //});
    }
}
