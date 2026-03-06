using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options) { }

    public DbSet<Virement> Virements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Virement>(entity =>
        {
            entity.HasKey(v => v.VirementId);
            entity.Property(v => v.Montant).HasPrecision(18, 2);
            entity.Property(v => v.DateVirement).IsRequired();
            entity.Property(v => v.Statut).IsRequired().HasMaxLength(50);
        });
    }
}
