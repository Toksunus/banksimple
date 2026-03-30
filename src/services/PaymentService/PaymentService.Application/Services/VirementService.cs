using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Services;

public class VirementService
{
    private readonly IVirementRepository _virementRepository;
    private readonly IAccountServiceClient _accountServiceClient;
    private readonly IVirementSagaOrchestrator _sagaOrchestrator;

    public VirementService(IVirementRepository virementRepository, IAccountServiceClient accountServiceClient, IVirementSagaOrchestrator sagaOrchestrator)
    {
        _virementRepository = virementRepository;
        _accountServiceClient = accountServiceClient;
        _sagaOrchestrator = sagaOrchestrator;
    }

    public async Task<List<Virement>> GetByCompteIdAsync(Guid compteId)
    {
        return await _virementRepository.GetByCompteIdAsync(compteId);
    }

    public async Task<Virement> EffectuerVirementAsync(VirementRequest command)
    {
        var compteSource = await _accountServiceClient.GetCompteAsync(command.CompteSourceId);
        var compteDestinataire = await _accountServiceClient.GetCompteAsync(command.CompteDestinataireId);

        if (compteSource == null || compteDestinataire == null)
            throw new Exception("Compte source ou destination introuvable.");

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

        return await _sagaOrchestrator.VirementAsync(command, estSuspect);
    }
}
