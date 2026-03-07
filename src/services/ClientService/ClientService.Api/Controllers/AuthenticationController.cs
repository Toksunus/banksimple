using ClientService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace ClientService.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationService _authService;
    private readonly IConnectionMultiplexer _redis;

    public AuthenticationController(AuthenticationService authService, IConnectionMultiplexer redis)
    {
        _authService = authService;
        _redis = redis;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var clientId = await _authService.VerifierCredentialsAsync(request);
            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            await _redis.GetDatabase().StringSetAsync($"otp:{clientId}", otpCode, TimeSpan.FromMinutes(5));
            return Ok(new { clientId, otpCode });
        }
        catch (Exception ex)
        {
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
                return Unauthorized(new { error = "Code MFA invalide ou expiré." });

            await db.KeyDeleteAsync($"otp:{request.ClientId}");

            var (session, token) = await _authService.CreerSessionAsync(request.ClientId);
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
