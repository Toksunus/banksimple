using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

public interface IVirementRepository
{
    Task<Virement?> GetByIdAsync(Guid virementId);
    Task<Virement> CreateAsync(Virement virement);
    Task<List<Virement>> GetByCompteIdAsync(Guid compteId);
}
