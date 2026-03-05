using BankSimple.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace BankSimple.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/clients/{clientId:guid}/comptes")]
public class CompteController : ControllerBase
{
    private readonly CompteService _compteService;
    private readonly IConnectionMultiplexer _redis;

    public CompteController(CompteService compteService, IConnectionMultiplexer redis)
    {
        _compteService = compteService;
        _redis = redis;
    }

    [HttpPost]
    public async Task<IActionResult> CreerCompte(Guid clientId, [FromBody] CompteRequest request)
    {
        try
        {
            request.ClientId = clientId;
            var compte = await _compteService.CreerCompteAsync(request);
            await _redis.GetDatabase().KeyDeleteAsync($"comptes:{clientId}");
            return CreatedAtAction(nameof(GetCompteById), new { clientId, compteId = compte.CompteId }, new
            {
                compte.CompteId,
                compte.Type,
                compte.Solde,
                compte.DateOuverture,
                compte.Statut,
                message = "Compte créé avec succès."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetComptes(Guid clientId)
    {
        var db = _redis.GetDatabase();
        var cacheKey = $"comptes:{clientId}";

        var cached = await db.StringGetAsync(cacheKey);
        if (cached.HasValue)
            return Content(cached!, "application/json");

        var comptes = await _compteService.GetComptesByClientIdAsync(clientId);
        var result = comptes.Select(compte => new
        {
            compte.CompteId,
            compte.Type,
            compte.Solde,
            compte.Statut,
            compte.DateOuverture
        });

        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(result), TimeSpan.FromSeconds(60));
        return Ok(result);
    }

    [HttpPost("{compteId:guid}/depot")]
    public async Task<IActionResult> Depot(Guid clientId, Guid compteId, [FromBody] decimal montant)
    {
        try
        {
            var compte = await _compteService.DepotAsync(compteId, montant);
            await _redis.GetDatabase().KeyDeleteAsync($"comptes:{clientId}");
            return Ok(new { compte.CompteId, compte.Solde, message = "Dépôt reussis" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{compteId:guid}")]
    public async Task<IActionResult> GetCompteById(Guid clientId, Guid compteId)
    {
        var compte = await _compteService.GetCompteByIdAsync(compteId);
        if (compte == null) return NotFound(new { error = "Compte existe pas." });
        if (compte.ClientId != clientId) return NotFound(new { error = "Compte existe pas." });

        return Ok(new
        {
            compte.CompteId,
            compte.Type,
            compte.Solde,
            compte.Statut,
            compte.DateOuverture
        });
    }
}