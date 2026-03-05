namespace PaymentService.Application.Services;

public class CompteDto
{
    public Guid CompteId { get; set; }
    public Guid ClientId { get; set; }
    public decimal Solde { get; set; }
    public string Statut { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public interface IAccountServiceClient
{
    Task<CompteDto?> GetCompteAsync(Guid compteId);
    Task<CompteDto> DebitAsync(Guid compteId, decimal montant);
    Task<CompteDto> CreditAsync(Guid compteId, decimal montant);
}
