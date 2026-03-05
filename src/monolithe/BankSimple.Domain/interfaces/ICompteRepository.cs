namespace BankSimple.Domain.Interfaces;

using BankSimple.Domain.Entities;

public interface ICompteRepository
{
    Task<Compte?> GetByIdAsync(Guid compteId);
    Task<Compte> CreateAsync(Compte compte);
    Task UpdateAsync(Compte compte);
    Task DeleteAsync(Guid compteId);
    Task<List<Compte>> GetByClientIdAsync(Guid clientId);
}