using BankSimple.Domain.Entities;
using BankSimple.Domain.Interfaces;

namespace BankSimple.Application.Services;

public enum TypeCompte { Cheque, Epargne }

public class CompteRequest
{
    public Guid ClientId { get; set; }
    public TypeCompte Type { get; set; }
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
        var compte = new Compte
        {
            CompteId = Guid.NewGuid(),
            ClientId = command.ClientId,
            Type = command.Type.ToString(),
            Solde = 0,
            DateOuverture = DateTime.UtcNow,
            Statut = "Actif"
        };

        return await _compteRepository.CreateAsync(compte);
    }

    public async Task<Compte?> GetCompteByIdAsync(Guid compteId)
    {
        return await _compteRepository.GetByIdAsync(compteId);
    }

    public async Task<List<Compte>> GetComptesByClientIdAsync(Guid clientId)
    {
        return await _compteRepository.GetByClientIdAsync(clientId);
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
}
