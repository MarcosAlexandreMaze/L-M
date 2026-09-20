using LMStore.Application.DTOs.Auth;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LMStore.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("registrar")]
    public async Task<ActionResult<LoginResponse>> Registrar(RegistrarClienteRequest request, CancellationToken ct)
    {
        var resultado = await authService.RegistrarClienteAsync(request, ct);
        DefinirCookieDeRefreshToken(resultado.RefreshToken, resultado.RefreshTokenExpiraEm);

        return StatusCode(201, new LoginResponse(resultado.AccessToken, resultado.AccessTokenExpiraEm));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var resultado = await authService.LoginAsync(request, ct);
        DefinirCookieDeRefreshToken(resultado.RefreshToken, resultado.RefreshTokenExpiraEm);

        return Ok(new LoginResponse(resultado.AccessToken, resultado.AccessTokenExpiraEm));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshTokenBruto) || string.IsNullOrEmpty(refreshTokenBruto))
            return Unauthorized();

        var resultado = await authService.RefreshAsync(refreshTokenBruto, ct);
        DefinirCookieDeRefreshToken(resultado.RefreshToken, resultado.RefreshTokenExpiraEm);

        return Ok(new LoginResponse(resultado.AccessToken, resultado.AccessTokenExpiraEm));
    }

    // httpOnly: inacessível via JavaScript no navegador (mitiga roubo de token por XSS).
    // Secure: só trafega em HTTPS. SameSite=Strict: não é enviado em requisições
    // originadas de outro site (mitiga CSRF). É o trio padrão atual para refresh token
    // em cookie — a "proteção de cookies" pedida.
    private void DefinirCookieDeRefreshToken(string token, DateTime expiraEm) =>
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiraEm
        });
}
