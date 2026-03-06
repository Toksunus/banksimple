using BankSimple.Domain.Entities;

namespace BankSimple.Domain.Interfaces;

public interface IVirementRepository
{
    Task<Virement?> GetByIdAsync(Guid virementId);
    Task<Virement> CreateAsync(Virement virement, Compte source, Compte destinataire);
    Task<List<Virement>> GetByCompteIdAsync(Guid compteId);
}