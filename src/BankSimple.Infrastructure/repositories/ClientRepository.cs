using BankSimple.Domain.Entities;
using BankSimple.Domain.Interfaces;
using BankSimple.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSimple.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly BankSimpleDbContext _context;

    public ClientRepository(BankSimpleDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid clientId)
    {
        return await _context.Clients
            .Include(c => c.Authentification)
            .Include(c => c.VerificationKYC)
            .Include(c => c.Sessions)
            .Include(c => c.ComptesBancaires)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);
    }

    public async Task<Client> CreateAsync(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task UpdateAsync(Client client)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid clientId)
    {
        var client = await GetByIdAsync(clientId);
        if (client != null)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Authentifications.AnyAsync(a => a.Login == email);
    }
}