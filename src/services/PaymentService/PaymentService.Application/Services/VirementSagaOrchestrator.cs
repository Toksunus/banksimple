using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Services;

public class VirementSagaOrchestrator : IVirementSagaOrchestrator

{
    private readonly IVirementSagaRepository _sagaRepository;
    private readonly IVirementRepository _virementRepository;
    private readonly IAccountServiceClient _accountServiceClient;

    public VirementSagaOrchestrator(
        IVirementSagaRepository sagaRepository,
        IVirementRepository virementRepository,
        IAccountServiceClient accountServiceClient)
    {
        _sagaRepository = sagaRepository;
        _virementRepository = virementRepository;
        _accountServiceClient = accountServiceClient;
    }

    public async Task<Virement> VirementAsync(VirementRequest request, bool estSuspect)
    {
        var saga = await _sagaRepository.CreateAsync(new VirementSaga
        {
            CompteSourceId = request.CompteSourceId,
            CompteDestinataireId = request.CompteDestinataireId,
            Montant = request.Montant
        });

        // Étape 1 : Débit
        try
        {
            await _accountServiceClient.DebitAsync(request.CompteSourceId, request.Montant);
            saga.Etape = "DebitEffectué";
            saga.UpdatedAt = DateTime.UtcNow;
            await _sagaRepository.UpdateAsync(saga);
        }
        catch
        {
            saga.Etape = "DebitEchoué";
            saga.UpdatedAt = DateTime.UtcNow;
            await _sagaRepository.UpdateAsync(saga);

            return await _virementRepository.CreateAsync(new Virement
            {
                VirementId = Guid.NewGuid(),
                CompteSourceId = request.CompteSourceId,
                CompteDestinataireId = request.CompteDestinataireId,
                Montant = request.Montant,
                DateVirement = DateTime.UtcNow,
                Statut = "Echoué"
            });
        }

        // Étape 2 : Crédit
        try
        {
            await _accountServiceClient.CreditAsync(request.CompteDestinataireId, request.Montant);
            saga.Etape = "CréditEffectué";
            saga.UpdatedAt = DateTime.UtcNow;
            await _sagaRepository.UpdateAsync(saga);
        }
        catch
        {
            saga.Etape = "CréditEchoué";
            saga.UpdatedAt = DateTime.UtcNow;
            await _sagaRepository.UpdateAsync(saga);

            // Compensation : rembourser le compte source
            try
            {
                await _accountServiceClient.CreditAsync(request.CompteSourceId, request.Montant);
                saga.Etape = "CompensationEffectuée";
                saga.UpdatedAt = DateTime.UtcNow;
                await _sagaRepository.UpdateAsync(saga);
            }
            catch
            {
                saga.Etape = "CompensationEchouée";
                saga.UpdatedAt = DateTime.UtcNow;
                await _sagaRepository.UpdateAsync(saga);
            }

            return await _virementRepository.CreateAsync(new Virement
            {
                VirementId = Guid.NewGuid(),
                CompteSourceId = request.CompteSourceId,
                CompteDestinataireId = request.CompteDestinataireId,
                Montant = request.Montant,
                DateVirement = DateTime.UtcNow,
                Statut = "Echoué"
            });
        }

        return await _virementRepository.CreateAsync(new Virement
        {
            VirementId = Guid.NewGuid(),
            CompteSourceId = request.CompteSourceId,
            CompteDestinataireId = request.CompteDestinataireId,
            Montant = request.Montant,
            DateVirement = DateTime.UtcNow,
            Statut = estSuspect ? "Suspect" : "Effectué"
        });
    }
}
