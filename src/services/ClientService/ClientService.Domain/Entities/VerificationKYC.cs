namespace ClientService.Domain.Entities;

public class VerificationKYC
{
    public Guid VerificationKYCId { get; set; }
    public Guid ClientId { get; set; }
    public DateTime DateVerification { get; set; }
    public bool Resultat { get; set; }

    public Client? Client { get; set; }
}
