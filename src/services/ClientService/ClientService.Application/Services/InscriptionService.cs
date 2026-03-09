using ClientService.Domain.Entities;
using ClientService.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ClientService.Application.Services;

public class InscriptionRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NasSimule { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}

public class InscriptionService
{
    private readonly IClientRepository _clientRepository;

    public InscriptionService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<Client> InscrireAsync(InscriptionRequest command)
    {
        if (await _clientRepository.ExistsByNasAsync(command.NasSimule))
            throw new Exception("Un client avec ce NAS existe déjà.");

        var client = new Client
        {
            ClientId = Guid.NewGuid(),
            Nom = command.Nom,
            Email = command.Email,
            NasSimule = command.NasSimule,
            Statut = "Pending",
            Authentification = new Authentification
            {
                AuthentificationId = Guid.NewGuid(),
                Login = command.NasSimule,
                MotDePasse = HashPassword(command.MotDePasse),
                MfaActive = true
            },
            VerificationKYC = new VerificationKYC
            {
                VerificationKYCId = Guid.NewGuid(),
                DateVerification = DateTime.UtcNow,
                Resultat = false
            }
        };

        return await _clientRepository.CreateAsync(client);
    }

    public async Task<Client?> ValiderKycAsync(Guid clientId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null) return null;

        if (client.VerificationKYC != null)
        {
            client.VerificationKYC.Resultat = true;
            client.VerificationKYC.DateVerification = DateTime.UtcNow;
        }

        client.Statut = "Active";
        await _clientRepository.UpdateAsync(client);
        return client;
    }

    public async Task<Client?> GetClientAsync(Guid clientId)
    {
        return await _clientRepository.GetByIdAsync(clientId);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
