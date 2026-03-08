using ClientService.Application.Services;
using ClientService.Domain.Entities;
using ClientService.Domain.Interfaces;
using Moq;
using Xunit;

namespace ClientService.Tests;

public class InscriptionServiceTests
{
    private readonly Mock<IClientRepository> _repoMock = new();
    private readonly InscriptionService _service;

    public InscriptionServiceTests()
    {
        _service = new InscriptionService(_repoMock.Object);
    }

    // NAS déjà utilisé
    [Fact]
    public async Task inscrire_duplication()
    {
        _repoMock.Setup(r => r.ExistsByNasAsync("10123456789")).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.InscrireAsync(new InscriptionRequest
            {
                Nom = "Daniel Atik", Email = "daniel.atik@gmail.com", NasSimule = "10123456789", MotDePasse = "Test1"
            }));

        Assert.Contains("NAS existe déjà", ex.Message);
    }

    // Nouveau client (statut Pending, KYC non rempli)
    [Fact]
    public async Task inscrire_statut_pending()
    {
        _repoMock.Setup(r => r.ExistsByNasAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Client>()))
                 .ReturnsAsync((Client c) => c);

        var result = await _service.InscrireAsync(new InscriptionRequest
        {
            Nom = "Jad Bizri", Email = "jad.bizri@gmail.com", NasSimule = "20123456789", MotDePasse = "Test2"
        });

        Assert.Equal("Pending", result.Statut);
    }

    // Mot de passe doit être hashé
    [Fact]
    public async Task inscrire_mot_de_passe_hashe()
    {
        _repoMock.Setup(r => r.ExistsByNasAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Client>()))
                 .ReturnsAsync((Client c) => c);

        var result = await _service.InscrireAsync(new InscriptionRequest
        {
            Nom = "Jad Bizri", Email = "jad.bizri@gmail.com", NasSimule = "20123456789", MotDePasse = "Test2"
        });

        Assert.NotNull(result.Authentification);
        Assert.NotEqual("Test2", result.Authentification!.MotDePasse);
    }

    // KYC non rempli à l'inscription
    [Fact]
    public async Task kyc_initial_false()
    {
        _repoMock.Setup(r => r.ExistsByNasAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Client>()))
                 .ReturnsAsync((Client c) => c);

        var result = await _service.InscrireAsync(new InscriptionRequest
        {
            Nom = "Jad Bizri", Email = "jad.bizri@gmail.com", NasSimule = "20123456789", MotDePasse = "Test2"
        });

        Assert.NotNull(result.VerificationKYC);
        Assert.False(result.VerificationKYC!.Resultat);
    }

    // Client introuvable lors de la validation KYC
    [Fact]
    public async Task client_introuvable()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Client?)null);

        var result = await _service.ValiderKycAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // KYC validé (statut passe à Active)
    [Fact]
    public async Task kyc_statut_actif()
    {
        var client = new Client
        {
            ClientId = Guid.NewGuid(),
            Nom = "Jad Bizri",
            Statut = "Pending",
            VerificationKYC = new VerificationKYC { Resultat = false, DateVerification = DateTime.UtcNow }
        };
        _repoMock.Setup(r => r.GetByIdAsync(client.ClientId)).ReturnsAsync(client);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Client>())).Returns(Task.CompletedTask);

        var result = await _service.ValiderKycAsync(client.ClientId);

        Assert.NotNull(result);
        Assert.Equal("Active", result!.Statut);
        Assert.True(result.VerificationKYC!.Resultat);
    }
}
