using BankSimple.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankSimple.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationService _authService;

    public AuthenticationController(AuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var (session, token) = await _authService.LoginAsync(request);
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
