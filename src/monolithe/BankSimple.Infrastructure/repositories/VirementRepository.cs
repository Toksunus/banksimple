using BankSimple.Domain.Entities;
using BankSimple.Domain.Interfaces;
using BankSimple.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSimple.Infrastructure.Repositories;

public class VirementRepository : IVirementRepository
{
    private readonly BankSimpleDbContext _context;

    public VirementRepository(BankSimpleDbContext context)
    {
        _context = context;
    }

    public async Task<Virement?> GetByIdAsync(Guid virementId)
    {
        return await _context.Virements.FindAsync(virementId);
    }

    public async Task<Virement> CreateAsync(Virement virement, Compte source, Compte destinataire)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Comptes.Update(source);
            _context.Comptes.Update(destinataire);
            _context.Virements.Add(virement);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return virement;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Virement>> GetByCompteIdAsync(Guid compteId)
    {
        return await _context.Virements
            .Where(virement => virement.CompteSourceId == compteId || virement.CompteDestinataireId == compteId)
            .ToListAsync();
    }
}