using PaymentService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace PaymentService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/virements")]
public class VirementController : ControllerBase
{
    private readonly VirementService _virementService;
    private readonly IAccountServiceClient _accountServiceClient;

    public VirementController(VirementService virementService, IAccountServiceClient accountServiceClient)
    {
        _virementService = virementService;
        _accountServiceClient = accountServiceClient;
    }

    [HttpGet("compte/{compteId:guid}")]
    public async Task<IActionResult> GetByCompte(Guid compteId)
    {
        var virements = await _virementService.GetByCompteIdAsync(compteId);

        var contrepartieIds = virements
            .Select(virement => virement.CompteSourceId == compteId ? virement.CompteDestinataireId : virement.CompteSourceId)
            .Distinct()
            .ToList();

        var compteClientMap = new ConcurrentDictionary<Guid, Guid?>();
        await Task.WhenAll(contrepartieIds.Select(async id =>
        {
            var compte = await _accountServiceClient.GetCompteAsync(id);
            compteClientMap[id] = compte?.ClientId;
        }));

        return Ok(virements.OrderByDescending(virement => virement.DateVirement).Select(virement =>
        {
            var contrepartieId = virement.CompteSourceId == compteId ? virement.CompteDestinataireId : virement.CompteSourceId;
            compteClientMap.TryGetValue(contrepartieId, out var titulaireId);
            return new
            {
                virement.VirementId,
                virement.CompteSourceId,
                virement.CompteDestinataireId,
                virement.Montant,
                virement.DateVirement,
                virement.Statut,
                TitulaireId = titulaireId
            };
        }));
    }

    [HttpPost]
    public async Task<IActionResult> EffectuerVirement([FromBody] VirementRequest request)
    {
        try
        {
            var virement = await _virementService.EffectuerVirementAsync(request);
            return Ok(new
            {
                virement.VirementId,
                virement.CompteSourceId,
                virement.CompteDestinataireId,
                virement.Montant,
                virement.DateVirement,
                virement.Statut
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
