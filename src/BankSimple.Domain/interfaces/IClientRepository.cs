namespace BankSimple.Domain.Interfaces;

using BankSimple.Domain.Entities;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid clientId);
    Task<Client> CreateAsync(Client client);
    Task UpdateAsync(Client client);
    Task DeleteAsync(Guid clientId);
    Task<bool> ExistsByEmailAsync(string email);
}