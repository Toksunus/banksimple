using BankSimple.Domain.Entities;
using BankSimple.Domain.Interfaces;

namespace BankSimple.Application.Commands;

public class CreateClientCommand
{
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string NasSimule { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}

public class UpdateClientCommand
{
    public Guid ClientId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
}

public class DeleteClientCommand
{
    public Guid ClientId { get; set; }
}

// Command Handlers

public class WriteClientHandler
{
    private readonly IClientRepository _clientRepository;

    public WriteClientHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Client> HandleCreateAsync(CreateClientCommand command)
    {

        if (await _clientRepository.ExistsByEmailAsync(command.NasSimule))
        {
            throw new Exception("Un client avec ce NAS existe déjà.");
        }

        var client = new Client
        {
            ClientId = Guid.NewGuid(),
            Nom = command.Nom,
            Adresse = command.Adresse,
            NasSimule = command.NasSimule,
            Statut = "Pending"
        };

        return await _clientRepository.CreateAsync(client);
    }

    public async Task<Client?> HandleUpdateAsync(UpdateClientCommand command)
    {
        var client = await _clientRepository.GetByIdAsync(command.ClientId);
        if (client == null) return null;

        client.Nom = command.Nom;
        client.Adresse = command.Adresse;
        client.NasSimule = command.NasSimule;
        client.Statut = command.Statut;

        return await _clientRepository.UpdateAsync(client);
    }

    public async Task<bool> HandleDeleteAsync(DeleteClientCommand command)
    {
        var client = await _clientRepository.GetByIdAsync(command.ClientId);
        if (client == null) return false;

        await _clientRepository.DeleteAsync(command.ClientId);

        return true;
    }
}

