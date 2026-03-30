namespace PaymentService.Domain.Entities;

public class VirementRequest
{
    public Guid CompteSourceId { get; set; }
    public Guid CompteDestinataireId { get; set; }
    public decimal Montant { get; set; }
}
