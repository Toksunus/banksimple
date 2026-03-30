using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

public interface IVirementSagaOrchestrator
{
    Task<Virement> VirementAsync(VirementRequest request, bool estSuspect);
}
