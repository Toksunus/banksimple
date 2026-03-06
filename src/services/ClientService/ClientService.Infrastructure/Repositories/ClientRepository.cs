using ClientService.Domain.Entities;
using ClientService.Domain.Interfaces;
using ClientService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly ClientDbContext _context;

    public ClientRepository(ClientDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid clientId)
    {
        return await _context.Clients
            .Include(client => client.Authentification)
            .Include(client => client.VerificationKYC)
            .Include(client => client.Sessions)
            .FirstOrDefaultAsync(client => client.ClientId == clientId);
    }

    public async Task<Client> CreateAsync(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task UpdateAsync(Client client)
    {
        if (_context.Entry(client).State == EntityState.Detached)
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
        return await _context.Authentifications.AnyAsync(authentification => authentification.Login == email);
    }

    public async Task<bool> ExistsByNasAsync(string nas)
    {
        return await _context.Clients.AnyAsync(client => client.NasSimule == nas);
    }

    public async Task<Client?> GetByLoginAsync(string login)
    {
        return await _context.Clients
            .Include(client => client.Authentification)
            .FirstOrDefaultAsync(client => client.Authentification != null && client.Authentification.Login == login);
    }

    public async Task<Session> CreateSessionAsync(Session session)
    {
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }
}
