namespace ClientService.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Action { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTime DateHeure { get; set; } = DateTime.UtcNow;
}
