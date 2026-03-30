using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options) { }

    public DbSet<Virement> Virements { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<VirementSaga> VirementSagas { get; set; }

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

        modelBuilder.Entity<VirementSaga>(entity =>
        {
            entity.HasKey(s => s.SagaId);
            entity.Property(s => s.Montant).HasPrecision(18, 2);
            entity.Property(s => s.Etape).IsRequired().HasMaxLength(50);
        });
    }
}
