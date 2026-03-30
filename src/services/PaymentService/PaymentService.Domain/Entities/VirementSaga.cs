namespace PaymentService.Domain.Entities;

public class VirementSaga
{
    public Guid SagaId { get; set; } = Guid.NewGuid();
    public Guid CompteSourceId { get; set; }
    public Guid CompteDestinataireId { get; set; }
    public decimal Montant { get; set; }
    public string Etape { get; set; } = "Démarré";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
