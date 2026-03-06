namespace BankSimple.Domain.Entities;

public class Compte
{
    public Guid CompteId { get; set; }
    public Guid ClientId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Solde { get; set; } = 0;
    public DateTime DateOuverture { get; set; }
    public string Statut { get; set; } = "Actif";

    public Client? Client { get; set; }
}