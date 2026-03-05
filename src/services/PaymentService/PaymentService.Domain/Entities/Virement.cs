namespace PaymentService.Domain.Entities;

public class Virement
{
    public Guid VirementId { get; set; }
    public Guid CompteSourceId { get; set; }
    public Guid CompteDestinataireId { get; set; }
    public decimal Montant { get; set; }
    public DateTime DateVirement { get; set; }
    public string Statut { get; set; } = "En attente";
}
