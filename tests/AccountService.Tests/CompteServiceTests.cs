using AccountService.Application.Services;
using AccountService.Domain.Entities;
using AccountService.Domain.Interfaces;
using Moq;
using Xunit;

namespace AccountService.Tests;

public class CompteServiceTests
{
    private readonly Mock<ICompteRepository> _repoMock = new();
    private readonly CompteService _service;

    public CompteServiceTests()
    {
        _service = new CompteService(_repoMock.Object);
    }

    // Nouveau compte (solde doit être 0, statut doit être Actif)
    [Fact]
    public async Task creer_compte()
    {
        var clientId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByClientIdAsync(clientId))
                 .ReturnsAsync(new List<Compte>());
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Compte>()))
                 .ReturnsAsync((Compte c) => c);

        var result = await _service.CreerCompteAsync(new CompteRequest
        {
            ClientId = clientId, Type = TypeCompte.Cheque
        });

        Assert.Equal(0, result.Solde);
        Assert.Equal("Actif", result.Statut);
    }

    // créer compte: dépôt 50 000$, débit, fonds insuffisants, dépôt négatif
    [Fact]
    public async Task compte_depot_debit()
    {
        var jadId    = Guid.NewGuid();
        var compteId = Guid.NewGuid();
        var compte   = new Compte { CompteId = compteId, ClientId = jadId, Solde = 0, Statut = "Actif", Type = "Cheque" };

        _repoMock.Setup(r => r.GetByClientIdAsync(jadId)).ReturnsAsync(new List<Compte>());
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Compte>())).ReturnsAsync((Compte c) => c);
        _repoMock.Setup(r => r.GetByIdAsync(compteId)).ReturnsAsync(compte);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Compte>())).Returns(Task.CompletedTask);

        var created = await _service.CreerCompteAsync(new CompteRequest { ClientId = jadId, Type = TypeCompte.Cheque });
        Assert.Equal(0, created.Solde);
        Assert.Equal("Cheque", created.Type);

        // Dépôt initial (50 000$)
        var apresDepot = await _service.DepotAsync(compteId, 50000);
        Assert.Equal(50000, apresDepot.Solde);

        var apresDebit = await _service.DebitAsync(compteId, 300);
        Assert.Equal(49700, apresDebit.Solde);

        // Fonds insuffisants
        var soldeSup = await Assert.ThrowsAsync<Exception>(() =>
            _service.DebitAsync(compteId, 99999));
        Assert.Contains("Fonds insuffisants", soldeSup.Message);

        // Dépôt négatif interdit
        var soldeNeg = await Assert.ThrowsAsync<Exception>(() =>
            _service.DepotAsync(compteId, -50));
        Assert.Contains("supérieur à 0", soldeNeg.Message);
    }
}
