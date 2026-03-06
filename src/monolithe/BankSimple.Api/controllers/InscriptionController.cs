using BankSimple.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankSimple.Api.Controllers;

[ApiController]
[Route("api/v1/clients")]
public class InscriptionController : ControllerBase
{
    private readonly InscriptionService _inscriptionService;

    public InscriptionController(InscriptionService inscriptionService)
    {
        _inscriptionService = inscriptionService;
    }

    [HttpPost("inscription")]
    public async Task<IActionResult> Inscription([FromBody] InscriptionRequest inscription)
    {
        try
        {
            var client = await _inscriptionService.InscrireAsync(inscription);
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

        return Ok(new
        {
            client.ClientId,
            client.Nom,
            client.Statut,
            message = "KYC validé. Profil actif."
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id){
        var client = await _inscriptionService.GetClientAsync(id);
        if (client == null) return NotFound(new { error = "Client n'existe pas." });

        object? kyc = null;
        if (client.VerificationKYC != null){
            kyc = new
            {
                resultat = client.VerificationKYC.Resultat ? "Actif" : "En attente",
                client.VerificationKYC.DateVerification
            };
        }

        return Ok(new{
            client.ClientId,
            client.Nom,
            client.Adresse,
            client.Statut,
            kyc
        });
    }
}
