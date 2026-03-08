using ClientService.Application.Services;
using ClientService.Domain.Entities;
using ClientService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace ClientService.Api.Controllers;

[ApiController]
[Route("api/v1/clients")]
public class InscriptionController : ControllerBase
{
    private readonly InscriptionService _inscriptionService;
    private readonly ClientDbContext _db;

    public InscriptionController(InscriptionService inscriptionService, ClientDbContext db)
    {
        _inscriptionService = inscriptionService;
        _db = db;
    }

    [HttpPost("inscription")]
    public async Task<IActionResult> Inscription([FromBody] InscriptionRequest inscription)
    {
        try
        {
            var client = await _inscriptionService.InscrireAsync(inscription);

            await _db.AuditLogs.AddAsync(new AuditLog { Action = "INSCRIPTION", ClientId = client.ClientId, Details = $"Nom={client.Nom}" });
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = client.ClientId }, new
            {
                client.ClientId,
                client.Nom,
                client.Statut,
                message = "Profil client créé avec succés. En attente de validation KYC."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/valider-kyc")]
    public async Task<IActionResult> ValiderKyc(Guid id)
    {
        var client = await _inscriptionService.ValiderKycAsync(id);
        if (client == null) return NotFound(new { error = "Client n'existe pas." });

        await _db.AuditLogs.AddAsync(new AuditLog { Action = "KYC_VALIDE", ClientId = client.ClientId, Details = $"Statut={client.Statut}" });
        await _db.SaveChangesAsync();

        return Ok(new
        {
            client.ClientId,
            client.Nom,
            client.Statut,
            message = "KYC validé. Profil actif."
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = await _inscriptionService.GetClientAsync(id);
        if (client == null) return NotFound(new { error = "Client n'existe pas." });

        object? kyc = null;
        if (client.VerificationKYC != null)
        {
            kyc = new
            {
                resultat = client.VerificationKYC.Resultat ? "Actif" : "En attente",
                client.VerificationKYC.DateVerification
            };
        }

        return Ok(new
        {
            client.ClientId,
            client.Nom,
            client.Adresse,
            client.Statut,
            kyc
        });
    }
}
