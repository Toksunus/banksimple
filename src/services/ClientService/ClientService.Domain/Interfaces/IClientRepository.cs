using ClientService.Domain.Entities;

namespace ClientService.Domain.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid clientId);
    Task<Client> CreateAsync(Client client);
    Task UpdateAsync(Client client);
    Task DeleteAsync(Guid clientId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByNasAsync(string nas);
    Task<Client?> GetByLoginAsync(string login);
    Task<Session> CreateSessionAsync(Session session);
}
