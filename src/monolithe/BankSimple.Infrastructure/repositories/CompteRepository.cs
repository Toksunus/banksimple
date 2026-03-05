using BankSimple.Domain.Entities;
using BankSimple.Domain.Interfaces;
using BankSimple.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSimple.Infrastructure.Repositories;

public class CompteRepository : ICompteRepository
{
    private readonly BankSimpleDbContext _context;

    public CompteRepository(BankSimpleDbContext context)
    {
        _context = context;
    }

    public async Task<Compte?> GetByIdAsync(Guid compteId)
    {
        return await _context.Comptes
            .Include(compte => compte.Client)
            .FirstOrDefaultAsync(compte => compte.CompteId == compteId);
    }

    public async Task<Compte> CreateAsync(Compte compte)
    {
        _context.Comptes.Add(compte);
        await _context.SaveChangesAsync();
        return compte;
    }

    public async Task UpdateAsync(Compte compte)
    {
        _context.Comptes.Update(compte);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid compteId)
    {
        var compte = await GetByIdAsync(compteId);
        if (compte != null)
        {
            _context.Comptes.Remove(compte);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Compte>> GetByClientIdAsync(Guid clientId)
    {
        return await _context.Comptes
            .Where(compte => compte.ClientId == clientId)
            .ToListAsync();
    }
}