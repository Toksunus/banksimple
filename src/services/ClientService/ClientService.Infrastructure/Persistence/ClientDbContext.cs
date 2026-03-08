using Microsoft.EntityFrameworkCore;
using ClientService.Domain.Entities;

namespace ClientService.Infrastructure.Persistence;

public class ClientDbContext : DbContext
{
    public ClientDbContext(DbContextOptions<ClientDbContext> options)
        : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Authentification> Authentifications { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<VerificationKYC> VerificationKYCs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.ClientId);
            entity.Property(c => c.Nom).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Adresse).IsRequired().HasMaxLength(255);
            entity.Property(c => c.NasSimule).IsRequired().HasMaxLength(11);
            entity.Property(c => c.Statut).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Authentification>(entity =>
        {
            entity.HasKey(a => a.AuthentificationId);
            entity.Property(a => a.Login).IsRequired().HasMaxLength(200);
            entity.Property(a => a.MotDePasse).IsRequired().HasMaxLength(300);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.SessionId);
        });

        modelBuilder.Entity<VerificationKYC>(entity =>
        {
            entity.HasKey(v => v.VerificationKYCId);
        });
    }
}
