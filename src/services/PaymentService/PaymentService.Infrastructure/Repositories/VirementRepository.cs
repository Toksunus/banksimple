using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Repositories;

public class VirementRepository : IVirementRepository
{
    private readonly PaymentDbContext _context;

    public VirementRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Virement?> GetByIdAsync(Guid virementId)
    {
        return await _context.Virements.FindAsync(virementId);
    }

    public async Task<Virement> CreateAsync(Virement virement)
    {
        _context.Virements.Add(virement);
        await _context.SaveChangesAsync();
        return virement;
    }

    public async Task<List<Virement>> GetByCompteIdAsync(Guid compteId)
    {
        return await _context.Virements
            .Where(virement => virement.CompteSourceId == compteId || virement.CompteDestinataireId == compteId)
            .ToListAsync();
    }
}
