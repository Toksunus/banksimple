using Microsoft.EntityFrameworkCore;
using BankSimple.Domain.Entities;

namespace BankSimple.Infrastructure.Persistence;

public class BankSimpleDbContext : DbContext
{
    public BankSimpleDbContext(DbContextOptions<BankSimpleDbContext> options)
        : base(options) { }


    public DbSet<BankSimple.Domain.Entities.Client> Clients { get; set; }
    public DbSet<BankSimple.Domain.Entities.Authentification> Authentifications { get; set; }
    public DbSet<BankSimple.Domain.Entities.Session> Sessions { get; set; }
    public DbSet<BankSimple.Domain.Entities.VerificationKYC> VerificationKYCs { get; set; }
    public DbSet<BankSimple.Domain.Entities.CompteBancaire> CompteBancaires { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BankSimple.Domain.Entities.Client>(entity =>
        {
            entity.HasKey(c => c.ClientId);
            entity.Property(c => c.Nom).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Adresse).IsRequired().HasMaxLength(255);
            entity.Property(c => c.NasSimule).IsRequired().HasMaxLength(11);
            entity.Property(c => c.Statut).IsRequired().HasMaxLength(50);
        });
        modelBuilder.Entity<BankSimple.Domain.Entities.Authentification>(entity =>
        {
            entity.HasKey(a => a.AuthentificationId);
            entity.Property(a => a.Login).IsRequired().HasMaxLength(200);
            entity.Property(a => a.MotDePasse).IsRequired().HasMaxLength(300);
        });
        modelBuilder.Entity<BankSimple.Domain.Entities.CompteBancaire>(entity =>
        {
            entity.HasKey(cb => cb.CompteBancaireId);
            entity.Property(cb => cb.Type).IsRequired().HasMaxLength(50);
            entity.Property(cb => cb.Solde).HasPrecision(18,2);
            entity.Property(cb => cb.DateOuverture).IsRequired();
            entity.Property(cb => cb.Statut).IsRequired().HasMaxLength(50);
        });
        modelBuilder.Entity<BankSimple.Domain.Entities.Session>(entity =>
        {
            entity.HasKey(s => s.SessionId);
        });
        modelBuilder.Entity<BankSimple.Domain.Entities.VerificationKYC>(entity =>
        {
            entity.HasKey(v => v.VerificationKYCId);
        });
    }
}