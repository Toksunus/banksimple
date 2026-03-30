using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

public class VirementSagaRepository : IVirementSagaRepository
{
    private readonly PaymentDbContext _context;

    public VirementSagaRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<VirementSaga> CreateAsync(VirementSaga saga)
    {
        _context.VirementSagas.Add(saga);
        await _context.SaveChangesAsync();
        return saga;
    }

    public async Task UpdateAsync(VirementSaga saga)
    {
        _context.VirementSagas.Update(saga);
        await _context.SaveChangesAsync();
    }
}
