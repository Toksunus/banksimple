using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

public interface IVirementSagaRepository
{
    Task<VirementSaga> CreateAsync(VirementSaga saga);
    Task UpdateAsync(VirementSaga saga);
}
