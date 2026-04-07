using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

public interface IAccountServiceClient
{
    Task<CompteDto?> GetCompteAsync(Guid compteId);
    Task<CompteDto?> GetCompteByKeyAsync(string accountKey);
    Task<CompteDto?> GetCompteByBbcIdAsync(int bbcCompteId);
    Task<CompteDto> DebitAsync(Guid compteId, decimal montant);
    Task<CompteDto> CreditAsync(Guid compteId, decimal montant);
}
