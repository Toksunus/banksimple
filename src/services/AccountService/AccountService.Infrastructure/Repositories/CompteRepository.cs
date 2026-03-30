using AccountService.Domain.Entities;
using AccountService.Domain.Interfaces;
using AccountService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Repositories;

public class CompteRepository : ICompteRepository
{
    private readonly AccountDbContext _context;

    public CompteRepository(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<Compte?> GetByIdAsync(Guid compteId)
    {
        return await _context.Comptes
            .FirstOrDefaultAsync(c => c.CompteId == compteId);
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
            .Where(c => c.ClientId == clientId)
            .ToListAsync();
    }

    public async Task<Compte?> GetByKeyAsync(string key)
    {
        return await _context.Comptes
            .FirstOrDefaultAsync(c => c.Key == key);
    }
}
