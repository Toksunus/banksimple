using AccountService.Domain.Entities;
using AccountService.Domain.Interfaces;

namespace AccountService.Application.Services;

public enum TypeCompte { Cheque, Epargne }

public class CompteRequest
{
    public Guid ClientId { get; set; }
    public TypeCompte Type { get; set; }
    public string? Key { get; set; }
}

public class CompteService
{
    private readonly ICompteRepository _compteRepository;

    public CompteService(ICompteRepository compteRepository)
    {
        _compteRepository = compteRepository;
    }

    public async Task<Compte> CreerCompteAsync(CompteRequest command)
    {
        var existing = await _compteRepository.GetByClientIdAsync(command.ClientId);
        if (existing.Any(c => c.Type == command.Type.ToString()))
            throw new Exception($"Vous avez déjà un compte {command.Type}.");

        var compte = new Compte
        {
            CompteId = Guid.NewGuid(),
            ClientId = command.ClientId,
            Type = command.Type.ToString(),
            Solde = 0,
            DateOuverture = DateTime.UtcNow,
            Statut = "Actif",
            Key = command.Key ?? string.Empty
        };

        return await _compteRepository.CreateAsync(compte);
    }

    public async Task<Compte?> GetCompteByIdAsync(Guid compteId)
    {
        return await _compteRepository.GetByIdAsync(compteId);
    }

    public async Task<Compte?> GetCompteByKeyAsync(string key)
    {
        return await _compteRepository.GetByKeyAsync(key);
    }

    public async Task<Compte?> GetCompteByBbcIdAsync(int bbcCompteId)
    {
        return await _compteRepository.GetByBbcIdAsync(bbcCompteId);
    }

    public async Task<List<Compte>> GetComptesByClientIdAsync(Guid clientId)
    {
        return await _compteRepository.GetByClientIdAsync(clientId);
    }

    public async Task FermerCompteAsync(Guid compteId, Guid clientId)
    {
        var compte = await _compteRepository.GetByIdAsync(compteId);
        if (compte == null)
            throw new Exception("Compte introuvable.");
        if (compte.ClientId != clientId)
            throw new Exception("Compte introuvable.");
        if (compte.Solde != 0)
            throw new Exception("Le solde doit être à 0 avant de fermer le compte.");

        await _compteRepository.DeleteAsync(compteId);
    }

    public async Task<Compte> DepotAsync(Guid compteId, decimal montant)
    {
        var compte = await _compteRepository.GetByIdAsync(compteId);
        if (compte == null)
            throw new Exception("Compte introuvable.");
        if (montant <= 0)
            throw new Exception("Le montant doit être supérieur à 0.");

        compte.Solde += montant;
        await _compteRepository.UpdateAsync(compte);
        return compte;
    }

    public async Task<Compte> DebitAsync(Guid compteId, decimal montant)
    {
        var compte = await _compteRepository.GetByIdAsync(compteId);
        if (compte == null)
            throw new Exception("Compte introuvable.");
        if (montant <= 0)
            throw new Exception("Le montant doit être supérieur à 0.");
        if (compte.Solde < montant)
            throw new Exception("Fonds insuffisants sur le compte.");

        compte.Solde -= montant;
        await _compteRepository.UpdateAsync(compte);
        return compte;
    }

    public async Task<Compte> CreditAsync(Guid compteId, decimal montant)
    {
        var compte = await _compteRepository.GetByIdAsync(compteId);
        if (compte == null)
            throw new Exception("Compte introuvable.");
        if (montant <= 0)
            throw new Exception("Le montant doit être supérieur à 0.");

        compte.Solde += montant;
        await _compteRepository.UpdateAsync(compte);
        return compte;
    }
}
