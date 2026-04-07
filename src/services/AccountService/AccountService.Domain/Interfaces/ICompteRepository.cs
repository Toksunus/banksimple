using AccountService.Domain.Entities;

namespace AccountService.Domain.Interfaces;

public interface ICompteRepository
{
    Task<Compte?> GetByIdAsync(Guid compteId);
    Task<Compte> CreateAsync(Compte compte);
    Task UpdateAsync(Compte compte);
    Task DeleteAsync(Guid compteId);
    Task<List<Compte>> GetByClientIdAsync(Guid clientId);
    Task<Compte?> GetByKeyAsync(string key);
    Task<Compte?> GetByBbcIdAsync(int bbcCompteId);
}
