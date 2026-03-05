using BankSimple.Domain.Entities;
using BankSimple.Domain.Interfaces;

namespace BankSimple.Application.Services;

public class VirementRequest
{
    public Guid CompteSourceId { get; set; }
    public Guid CompteDestinataireId { get; set; }
    public decimal Montant { get; set; }
}

public class VirementService
{
    private readonly ICompteRepository _compteRepository;
    private readonly IVirementRepository _virementRepository;


    public VirementService(ICompteRepository compteRepository, IVirementRepository virementRepository)
    {
        _compteRepository = compteRepository;
        _virementRepository = virementRepository;
    }

    public async Task<Virement> EffectuerVirementAsync(VirementRequest command)
    {
        var compteSource = await _compteRepository.GetByIdAsync(command.CompteSourceId);
        var compteDestinataire = await _compteRepository.GetByIdAsync(command.CompteDestinataireId);

        if (compteSource == null)
            throw new Exception("Compte source introuvable.");

        if (compteDestinataire == null)
            throw new Exception("Compte destinataire introuvable.");

        if (compteSource.Solde < command.Montant)
            throw new Exception("Fonds insuffisants sur le compte source.");

        var virementInterne = compteSource.ClientId == compteDestinataire.ClientId;
        var pourcentage = command.Montant / compteSource.Solde;
        bool estSuspect = false;

        if (!virementInterne)
        {
            if (pourcentage > 0.60m || command.Montant > 10000)
                estSuspect = true;
        }

        compteSource.Solde -= command.Montant;
        compteDestinataire.Solde += command.Montant;

        var virement = new Virement
        {
            VirementId = Guid.NewGuid(),
            CompteSourceId = command.CompteSourceId,
            CompteDestinataireId = command.CompteDestinataireId,
            Montant = command.Montant,
            DateVirement = DateTime.UtcNow,
            Statut = estSuspect ? "Suspect" : "Effectué"
        };

        return await _virementRepository.CreateAsync(virement, compteSource, compteDestinataire);
    }

    
}