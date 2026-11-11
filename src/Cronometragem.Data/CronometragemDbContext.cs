using Cronometragem.Core.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.Data;

public class CronometragemDbContext : DbContext
{
    public DbSet<Atleta> Atletas => Set<Atleta>();
    public DbSet<Prova> Provas => Set<Prova>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Inscricao> Inscricoes => Set<Inscricao>();
    public DbSet<LeituraEvento> Leituras => Set<LeituraEvento>();

    public CronometragemDbContext(DbContextOptions<CronometragemDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Atleta>(e =>
        {
            e.ToTable("atletas");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id");
            e.Property(a => a.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
            e.Property(a => a.Documento).HasColumnName("documento").HasMaxLength(20);
            e.Property(a => a.DataNascimento).HasColumnName("data_nascimento");
            e.Property(a => a.Sexo).HasColumnName("sexo").HasMaxLength(1);
            e.Property(a => a.Telefone).HasColumnName("telefone").HasMaxLength(20);
            e.Property(a => a.Email).HasColumnName("email").HasMaxLength(150);
        });

        modelBuilder.Entity<Prova>(e =>
        {
            e.ToTable("provas");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
            e.Property(p => p.DataProva).HasColumnName("data_prova");
            e.Property(p => p.HoraLargadaOficial).HasColumnName("hora_largada_oficial");
            e.Ignore(p => p.LargadaOficialEm);
            e.Ignore(p => p.Categorias);
            e.Ignore(p => p.Inscricoes);
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("categorias");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.ProvaId).HasColumnName("prova_id");
            e.Property(c => c.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            e.Property(c => c.IdadeMin).HasColumnName("idade_min");
            e.Property(c => c.IdadeMax).HasColumnName("idade_max");
            e.Property(c => c.Sexo).HasColumnName("sexo").HasMaxLength(1);
            e.HasOne<Prova>().WithMany().HasForeignKey(c => c.ProvaId);
        });

        modelBuilder.Entity<Inscricao>(e =>
        {
            e.ToTable("inscricoes");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnName("id");
            e.Property(i => i.ProvaId).HasColumnName("prova_id");
            e.Property(i => i.AtletaId).HasColumnName("atleta_id");
            e.Property(i => i.NumeroPeito).HasColumnName("numero_peito").HasMaxLength(10).IsRequired();
            e.Property(i => i.CategoriaId).HasColumnName("categoria_id");
            e.Property(i => i.ChipId).HasColumnName("chip_id").HasMaxLength(50).IsRequired();
            e.Property(i => i.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);

            e.HasOne<Prova>().WithMany().HasForeignKey(i => i.ProvaId);
            e.HasOne<Atleta>().WithMany().HasForeignKey(i => i.AtletaId);
            e.HasOne<Categoria>().WithMany().HasForeignKey(i => i.CategoriaId);

            e.HasIndex(i => new { i.ProvaId, i.ChipId }).IsUnique();
            e.HasIndex(i => new { i.ProvaId, i.NumeroPeito }).IsUnique();
        });

        modelBuilder.Entity<LeituraEvento>(e =>
        {
            e.ToTable("leituras");
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasColumnName("id");
            e.Property(l => l.ProvaId).HasColumnName("prova_id");
            e.Property(l => l.ChipId).HasColumnName("chip_id").HasMaxLength(50).IsRequired();
            e.Property(l => l.PontoControle).HasColumnName("ponto_controle").HasConversion<string>().HasMaxLength(20);
            e.Property(l => l.DataHoraLeitura).HasColumnName("dta_hora_leitura");
            e.Property(l => l.OrigemLeitor).HasColumnName("origem_leitor").HasMaxLength(100);
            e.Property(l => l.CriadoEm).HasColumnName("criado_em");

            e.HasOne<Prova>().WithMany().HasForeignKey(l => l.ProvaId);
            e.HasIndex(l => new { l.ProvaId, l.ChipId, l.PontoControle });
        });
    }
}
