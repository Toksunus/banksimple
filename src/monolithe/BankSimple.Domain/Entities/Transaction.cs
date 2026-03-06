namespace BankSimple.Domain.Entities;

public class Transaction
{
    public Guid TransactionId { get; set; }
    public Guid CompteId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public DateTime DateTransaction { get; set; }
    public string Description { get; set; } = string.Empty;

    public Compte? Compte { get; set; }
}