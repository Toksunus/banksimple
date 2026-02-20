namespace BankSimple.Domain.Entities;

public class Client
{
    public Guid ClientId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string NasSimule { get; set; } = string.Empty;
    public string Statut { get; set; } = "Pending";

    public Authentification? Authentification { get; set; }
    public VerificationKYC? VerificationKYC { get; set; }
    public List<Session> Sessions { get; set; } = new();
    public List<CompteBancaire> ComptesBancaires { get; set; } = new();

}