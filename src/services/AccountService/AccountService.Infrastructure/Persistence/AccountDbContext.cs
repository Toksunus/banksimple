using Microsoft.EntityFrameworkCore;
using AccountService.Domain.Entities;

namespace AccountService.Infrastructure.Persistence;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options)
        : base(options) { }

    public DbSet<Compte> Comptes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Compte>(entity =>
        {
            entity.HasKey(cb => cb.CompteId);
            entity.Property(cb => cb.BbcCompteId).ValueGeneratedOnAdd();
            entity.HasIndex(cb => cb.BbcCompteId).IsUnique();
            entity.Property(cb => cb.Type).IsRequired().HasMaxLength(50);
            entity.Property(cb => cb.Solde).HasPrecision(18, 2);
            entity.Property(cb => cb.DateOuverture).IsRequired();
            entity.Property(cb => cb.Statut).IsRequired().HasMaxLength(50);
        });
    }
}
