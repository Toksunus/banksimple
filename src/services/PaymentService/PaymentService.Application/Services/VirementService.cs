using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Services;

public class VirementRequest
{
    public Guid CompteSourceId { get; set; }
    public Guid CompteDestinataireId { get; set; }
    public decimal Montant { get; set; }
}

public class VirementService
{
    private readonly IVirementRepository _virementRepository;
    private readonly IAccountServiceClient _accountServiceClient;

    public VirementService(IVirementRepository virementRepository, IAccountServiceClient accountServiceClient)
    {
        _virementRepository = virementRepository;
        _accountServiceClient = accountServiceClient;
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

        await _accountServiceClient.DebitAsync(command.CompteSourceId, command.Montant);

        try
        {
            await _accountServiceClient.CreditAsync(command.CompteDestinataireId, command.Montant);
        }
        catch
        {
            try
            {
                await _accountServiceClient.CreditAsync(command.CompteSourceId, command.Montant);
            }
            catch
            {
            }

            var virementEchoue = new Virement
            {
                VirementId = Guid.NewGuid(),
                CompteSourceId = command.CompteSourceId,
                CompteDestinataireId = command.CompteDestinataireId,
                Montant = command.Montant,
                DateVirement = DateTime.UtcNow,
                Statut = "Echoué"
            };

            return await _virementRepository.CreateAsync(virementEchoue);
        }

        var virement = new Virement
        {
            VirementId = Guid.NewGuid(),
            CompteSourceId = command.CompteSourceId,
            CompteDestinataireId = command.CompteDestinataireId,
            Montant = command.Montant,
            DateVirement = DateTime.UtcNow,
            Statut = estSuspect ? "Suspect" : "Effectué"
        };

        return await _virementRepository.CreateAsync(virement);
    }
}
