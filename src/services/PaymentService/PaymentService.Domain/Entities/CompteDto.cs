namespace PaymentService.Domain.Entities;

public class CompteDto
{
    public Guid CompteId { get; set; }
    public Guid ClientId { get; set; }
    public int BbcCompteId { get; set; }
    public decimal Solde { get; set; }
    public string Statut { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
