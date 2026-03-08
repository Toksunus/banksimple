using PaymentService.Application.Services;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using Moq;
using Xunit;

namespace PaymentService.Tests;

public class VirementServiceTests
{
    private readonly Mock<IVirementRepository>   _virementRepoMock  = new();
    private readonly Mock<IAccountServiceClient> _accountClientMock = new();
    private readonly VirementService _service;

    public VirementServiceTests()
    {
        _service = new VirementService(_virementRepoMock.Object, _accountClientMock.Object);
    }

    // Jad Bizri → Daniel Atik, solde initial 50 000$
    private static CompteDto Compte(Guid compteId, Guid clientId, decimal solde) =>
        new() { CompteId = compteId, ClientId = clientId, Solde = solde, Statut = "Actif", Type = "Cheque" };

    // Virement normal de 50$ (statut doit être Effectué, montant doit être 50$)
    [Fact]
    public async Task virement_normal_effectue()
    {
        var jadId      = Guid.NewGuid();
        var danielId   = Guid.NewGuid();
        var compteJad  = Guid.NewGuid();
        var compteDaniel = Guid.NewGuid();
        var source = Compte(compteJad,    jadId,    50000);
        var dest   = Compte(compteDaniel, danielId, 50000);

        _accountClientMock.Setup(a => a.GetCompteAsync(compteJad)).ReturnsAsync(source);
        _accountClientMock.Setup(a => a.GetCompteAsync(compteDaniel)).ReturnsAsync(dest);
        _accountClientMock.Setup(a => a.DebitAsync(compteJad,    50)).ReturnsAsync(source);
        _accountClientMock.Setup(a => a.CreditAsync(compteDaniel, 50)).ReturnsAsync(dest);
        _virementRepoMock.Setup(r => r.CreateAsync(It.IsAny<Virement>()))
                         .ReturnsAsync((Virement v) => v);

        var result = await _service.EffectuerVirementAsync(new VirementRequest
        {
            CompteSourceId = compteJad, CompteDestinataireId = compteDaniel, Montant = 50
        });

        Assert.Equal("Effectué", result.Statut);
        Assert.Equal(50, result.Montant);
    }

    // validations du virement: compte source introuvable, fonds insuffisants, virement externe > 60% du solde, virement externe > 10 000$, virement interne grand montant (jamais Suspect)
    [Fact]
    public async Task virement_validations()
    {
        var jadId      = Guid.NewGuid();
        var danielId   = Guid.NewGuid();
        var compteJad  = Guid.NewGuid();
        var compteDaniel = Guid.NewGuid();
        var source = Compte(compteJad,    jadId,    50000);
        var dest   = Compte(compteDaniel, danielId, 50000);

        _accountClientMock.Setup(a => a.GetCompteAsync(compteJad)).ReturnsAsync(source);
        _accountClientMock.Setup(a => a.GetCompteAsync(compteDaniel)).ReturnsAsync(dest);
        _accountClientMock.Setup(a => a.DebitAsync(compteJad,    It.IsAny<decimal>())).ReturnsAsync(source);
        _accountClientMock.Setup(a => a.CreditAsync(compteDaniel, It.IsAny<decimal>())).ReturnsAsync(dest);
        _virementRepoMock.Setup(r => r.CreateAsync(It.IsAny<Virement>()))
                         .ReturnsAsync((Virement v) => v);

        // 1. Compte source introuvable
        _accountClientMock.Setup(a => a.GetCompteAsync(It.IsAny<Guid>())).ReturnsAsync((CompteDto?)null);
        var exSource = await Assert.ThrowsAsync<Exception>(() =>
            _service.EffectuerVirementAsync(new VirementRequest
            {
                CompteSourceId = Guid.NewGuid(), CompteDestinataireId = Guid.NewGuid(), Montant = 50
            }));
        Assert.Contains("introuvable", exSource.Message);

        _accountClientMock.Setup(a => a.GetCompteAsync(compteJad)).ReturnsAsync(source);
        _accountClientMock.Setup(a => a.GetCompteAsync(compteDaniel)).ReturnsAsync(dest);

        // 2. Fonds insuffisants
        var fondsIns = await Assert.ThrowsAsync<Exception>(() =>
            _service.EffectuerVirementAsync(new VirementRequest
            {
                CompteSourceId = compteJad, CompteDestinataireId = compteDaniel, Montant = 99999
            }));
        Assert.Contains("Fonds insuffisants", fondsIns.Message);

        // 3. Virement externe > 60% du solde (statut Suspect AML)
        var plusQue60 = Compte(compteJad, jadId, 50000);
        _accountClientMock.Setup(a => a.GetCompteAsync(compteJad)).ReturnsAsync(plusQue60);
        var aml60 = await _service.EffectuerVirementAsync(new VirementRequest
        {
            CompteSourceId = compteJad, CompteDestinataireId = compteDaniel, Montant = 35000
        });
        Assert.Equal("Suspect", aml60.Statut);

        // 4. Virement externe > 10 000$ (statut Suspect AML)
        var plusQue10k = Compte(compteJad, jadId, 50000);
        _accountClientMock.Setup(a => a.GetCompteAsync(compteJad)).ReturnsAsync(plusQue10k);
        var aml10k = await _service.EffectuerVirementAsync(new VirementRequest
        {
            CompteSourceId = compteJad, CompteDestinataireId = compteDaniel, Montant = 15000
        });
        Assert.Equal("Suspect", aml10k.Statut);

        // 5. Virement interne (même client) grand montant (jamais Suspect)
        var sourceInterne = Compte(compteJad,    jadId, 50000);
        var destInterne   = Compte(compteDaniel, jadId, 0);
        _accountClientMock.Setup(a => a.GetCompteAsync(compteJad)).ReturnsAsync(sourceInterne);
        _accountClientMock.Setup(a => a.GetCompteAsync(compteDaniel)).ReturnsAsync(destInterne);
        var interne = await _service.EffectuerVirementAsync(new VirementRequest
        {
            CompteSourceId = compteJad, CompteDestinataireId = compteDaniel, Montant = 49000
        });
        Assert.Equal("Effectué", interne.Statut);
    }
}
