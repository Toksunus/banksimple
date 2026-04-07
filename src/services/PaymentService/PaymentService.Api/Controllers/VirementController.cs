using PaymentService.Application.Services;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PaymentService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/virements")]
public class VirementController : ControllerBase
{
    private readonly VirementService _virementService;
    private readonly IAccountServiceClient _accountServiceClient;
    private readonly IBbcServiceClient _bbcServiceClient;
    private readonly IDatabase _redis;
    private readonly PaymentDbContext _db;

    public VirementController(VirementService virementService, IAccountServiceClient accountServiceClient, IBbcServiceClient bbcServiceClient, IConnectionMultiplexer redis, PaymentDbContext db)
    {
        _virementService = virementService;
        _accountServiceClient = accountServiceClient;
        _bbcServiceClient = bbcServiceClient;
        _redis = redis.GetDatabase();
        _db = db;
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
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (idempotencyKey != null)
        {
            var cached = await _redis.StringGetAsync($"idempotency:{idempotencyKey}");
            if (cached.HasValue)
                return Ok(JsonSerializer.Deserialize<object>(cached!));
        }

        try
        {
            var virement = await _virementService.EffectuerVirementAsync(request);
            var result = new
            {
                virement.VirementId,
                virement.CompteSourceId,
                virement.CompteDestinataireId,
                virement.Montant,
                virement.DateVirement,
                virement.Statut
            };

            if (idempotencyKey != null)
                await _redis.StringSetAsync($"idempotency:{idempotencyKey}", JsonSerializer.Serialize(result), TimeSpan.FromHours(24));

            await _db.AuditLogs.AddAsync(new AuditLog
            {
                Action = "VIREMENT_" + virement.Statut.ToUpper(),
                Details = $"Source={virement.CompteSourceId} Dest={virement.CompteDestinataireId} Montant={virement.Montant}"
            });
            await _db.SaveChangesAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("comptes/{compteId:guid}/register-bbc")]
    public async Task<IActionResult> RegisterBbc(Guid compteId)
    {
        try
        {
            var compte = await _accountServiceClient.GetCompteAsync(compteId);
            if (compte == null)
                return NotFound(new { error = "Compte introuvable." });
            if (string.IsNullOrEmpty(compte.Key))
                return BadRequest(new { error = "Ce compte n'a pas de clé BBC." });

            await _bbcServiceClient.RegisterKeyAsync(compte.BbcCompteId, compte.Key);
            return Ok(new { message = $"Clé '{compte.Key}' enregistrée chez BBC." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("externe")]
    public async Task<IActionResult> EffectuerVirementExterne([FromBody] VirementRequest request)
    {
        if (request.CompteSourceId == Guid.Empty)
            return BadRequest(new { error = "Compte source est requis pour un virement externe." });
        if (string.IsNullOrEmpty(request.ToKey))
            return BadRequest(new { error = "ToKey est requis pour un virement externe." });

        try
        {
            var compte = await _accountServiceClient.GetCompteAsync(request.CompteSourceId);
            if (compte == null)
                return NotFound(new { error = "Compte source introuvable." });

            await _bbcServiceClient.TransactionsAsync(compte.BbcCompteId, request.ToKey, request.Montant);
            return Ok(new { message = "Transaction initiée chez BBC." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
