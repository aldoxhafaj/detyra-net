using EProtokoll.Api.Dtos;
using EProtokoll.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request)
    {
        var token = await _auth.LoginAsync(request);
        if (token == null)
        {
            return Unauthorized();
        }
        return Ok(token);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshRequest request)
    {
        var token = await _auth.RefreshAsync(request);
        if (token == null)
        {
            return Unauthorized();
        }
        return Ok(token);
    }
}
