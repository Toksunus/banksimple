using ClientService.Application.Services;
using ClientService.Domain.Entities;
using ClientService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace ClientService.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationService _authService;
    private readonly IConnectionMultiplexer _redis;
    private readonly ClientDbContext _db;

    public AuthenticationController(AuthenticationService authService, IConnectionMultiplexer redis, ClientDbContext db)
    {
        _authService = authService;
        _redis = redis;
        _db = db;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var clientId = await _authService.VerifierCredentialsAsync(request);
            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            await _redis.GetDatabase().StringSetAsync($"otp:{clientId}", otpCode, TimeSpan.FromMinutes(5));

            await _db.AuditLogs.AddAsync(new AuditLog { Action = "LOGIN_SUCCESS", ClientId = clientId, Details = $"NAS={request.Login}" });
            await _db.SaveChangesAsync();

            return Ok(new { clientId, otpCode });
        }
        catch (Exception ex)
        {
            await _db.AuditLogs.AddAsync(new AuditLog { Action = "LOGIN_FAILED", Details = $"NAS={request.Login} Erreur={ex.Message}" });
            await _db.SaveChangesAsync();
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        try
        {
            var db = _redis.GetDatabase();
            var stored = await db.StringGetAsync($"otp:{request.ClientId}");

            if (!stored.HasValue || stored.ToString() != request.OtpCode)
            {
                await _db.AuditLogs.AddAsync(new AuditLog { Action = "MFA_FAILED", ClientId = request.ClientId, Details = "Code OTP invalide ou expiré" });
                await _db.SaveChangesAsync();
                return Unauthorized(new { error = "Code MFA invalide ou expiré." });
            }

            await db.KeyDeleteAsync($"otp:{request.ClientId}");

            var (session, token) = await _authService.CreerSessionAsync(request.ClientId);

            await _db.AuditLogs.AddAsync(new AuditLog { Action = "MFA_SUCCESS", ClientId = request.ClientId, Details = $"Session={session.SessionId}" });
            await _db.SaveChangesAsync();

            return Ok(new
            {
                token,
                session.SessionId,
                session.ClientId,
                session.DateCreation,
                session.DateExpiration
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
