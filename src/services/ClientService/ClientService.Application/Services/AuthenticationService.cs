using ClientService.Domain.Entities;
using ClientService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ClientService.Application.Services;

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    public Guid ClientId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
}

public class AuthenticationService
{
    private readonly IClientRepository _clientRepository;
    private readonly IConfiguration _configuration;

    public AuthenticationService(IClientRepository clientRepository, IConfiguration configuration)
    {
        _clientRepository = clientRepository;
        _configuration = configuration;
    }

    public async Task<Guid> VerifierCredentialsAsync(LoginRequest request)
    {
        var client = await _clientRepository.GetByLoginAsync(request.Login);

        if (client == null || client.Authentification == null)
            throw new Exception("Identifiants invalides.");

        if (client.Authentification.MotDePasse != HashPasswordUni(request.MotDePasse))
            throw new Exception("Identifiants invalides.");

        if (client.Statut != "Active")
            throw new Exception("Compte non activé. KYC requis.");

        return client.ClientId;
    }

    public async Task<(Session session, string token)> CreerSessionAsync(Guid clientId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
            throw new Exception("Client introuvable.");

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            ClientId = client.ClientId,
            DateCreation = DateTime.UtcNow,
            DateExpiration = DateTime.UtcNow.AddHours(4)
        };

        await _clientRepository.CreateSessionAsync(session);
        var token = GenererToken(client, session);
        return (session, token);
    }

    // Méthode de hachage du token bidirectionnel (SHA256 trouvée dans la documentation en ligne)
    private SigningCredentials HashTokenBi()
    {
        var secret = _configuration["Jwt:Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    private string GenererToken(Client client, Session session)
    {
        var creds = HashTokenBi();

        var claims = new[]
        {
            new Claim("clientId", client.ClientId.ToString()),
            new Claim("sessionId", session.SessionId.ToString()),
            new Claim(ClaimTypes.Name, client.Nom)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: session.DateExpiration,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Méthode de hachage du mot de passe unidirectionnel (SHA256 trouvée dans la documentation en ligne)
    private static string HashPasswordUni(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
