using AccountService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace AccountService.Api.Controllers;

[ApiController]
[Route("internal/comptes")]
[AllowAnonymous]
public class DebitCreditController : ControllerBase
{
    private readonly CompteService _compteService;
    private readonly IConnectionMultiplexer _redis;

    public DebitCreditController(CompteService compteService, IConnectionMultiplexer redis)
    {
        _compteService = compteService;
        _redis = redis;
    }

    [HttpPost("{compteId:guid}/debit")]
    public async Task<IActionResult> Debit(Guid compteId, [FromBody] decimal montant)
    {
        try
        {
            var compte = await _compteService.DebitAsync(compteId, montant);
            await _redis.GetDatabase().KeyDeleteAsync($"comptes:{compte.ClientId}");
            return Ok(new { compte.CompteId, compte.Solde, compte.ClientId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{compteId:guid}/credit")]
    public async Task<IActionResult> Credit(Guid compteId, [FromBody] decimal montant)
    {
        try
        {
            var compte = await _compteService.CreditAsync(compteId, montant);
            await _redis.GetDatabase().KeyDeleteAsync($"comptes:{compte.ClientId}");
            return Ok(new { compte.CompteId, compte.Solde, compte.ClientId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{compteId:guid}")]
    public async Task<IActionResult> GetCompte(Guid compteId)
    {
        var compte = await _compteService.GetCompteByIdAsync(compteId);
        if (compte == null) return NotFound(new { error = "Compte introuvable." });

        return Ok(new
        {
            compte.CompteId,
            compte.ClientId,
            compte.Solde,
            compte.Statut,
            compte.Type
        });
    }
}
