using ClientService.Application.Services;
using ClientService.Domain.Entities;
using ClientService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ClientService.Tests;

public class AuthenticationServiceTests
{
    private readonly Mock<IClientRepository> _repoMock = new();
    private readonly AuthenticationService _service;

    // Configuration du token d'authentification pour les tests
    public AuthenticationServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]          = "BankSimple2026SuperSecretJwtKey!!",
                ["Jwt:Issuer"]          = "BankSimple",
                ["Jwt:Audience"]        = "BankSimpleClients",
                ["Jwt:ExpirationHours"] = "4",
            })
            .Build();

        _service = new AuthenticationService(_repoMock.Object, config);
    }

    // Création d'un client actif avec mot de passe hashé
    private static Client ClientActif(string nas, string motDePasse)
    {
        using var sha  = System.Security.Cryptography.SHA256.Create();
        var hash = Convert.ToBase64String(
            sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(motDePasse)));

        return new Client
        {
            ClientId         = Guid.NewGuid(),
            Nom              = "Jad Bizri",
            Statut           = "Active",
            Authentification = new Authentification { Login = nas, MotDePasse = hash }
        };
    }

    // Credentials valides
    [Fact]
    public async Task login_credentials_valides()
    {
        var client = ClientActif("20123456789", "Test2");
        _repoMock.Setup(r => r.GetByLoginAsync("20123456789")).ReturnsAsync(client);

        var clientId = await _service.VerifierCredentialsAsync(
            new LoginRequest { Login = "20123456789", MotDePasse = "Test2" });

        Assert.Equal(client.ClientId, clientId);
    }

    // Flow complet: mauvais credentials, KYC manquant, login valide, création de session JWT
    [Fact]
    public async Task login_mauvais_credentials()
    {
        var client = ClientActif("20123456789", "Test2");
        _repoMock.Setup(r => r.GetByLoginAsync("20123456789")).ReturnsAsync(client);
        _repoMock.Setup(r => r.GetByIdAsync(client.ClientId)).ReturnsAsync(client);
        _repoMock.Setup(r => r.CreateSessionAsync(It.IsAny<Session>()))
                 .ReturnsAsync((Session s) => s);

        // 1. Mauvais identifiants
        _repoMock.Setup(r => r.GetByLoginAsync("10123456789")).ReturnsAsync((Client?)null);
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _service.VerifierCredentialsAsync(
                new LoginRequest { Login = "10123456789", MotDePasse = "Test1" }));
        Assert.Contains("Identifiants invalides", ex.Message);

        // 2. Compte non validé KYC
        var clientPending = ClientActif("20123456789", "Test2");
        clientPending.Statut = "Pending";
        _repoMock.Setup(r => r.GetByLoginAsync("20123456789")).ReturnsAsync(clientPending);
        var exKyc = await Assert.ThrowsAsync<Exception>(() =>
            _service.VerifierCredentialsAsync(
                new LoginRequest { Login = "20123456789", MotDePasse = "Test2" }));
        Assert.Contains("KYC", exKyc.Message);

        // 3. Credentials valides
        _repoMock.Setup(r => r.GetByLoginAsync("20123456789")).ReturnsAsync(client);
        var clientId = await _service.VerifierCredentialsAsync(
            new LoginRequest { Login = "20123456789", MotDePasse = "Test2" });
        Assert.Equal(client.ClientId, clientId);

        // 4. Créer session
        var (session, token) = await _service.CreerSessionAsync(clientId);
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Equal(client.ClientId, session.ClientId);
        Assert.Equal(4, (int)(session.DateExpiration - session.DateCreation).TotalHours);
    }
}
