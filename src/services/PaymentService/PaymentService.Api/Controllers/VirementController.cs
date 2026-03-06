using PaymentService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/virements")]
public class VirementController : ControllerBase
{
    private readonly VirementService _virementService;

    public VirementController(VirementService virementService)
    {
        _virementService = virementService;
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
