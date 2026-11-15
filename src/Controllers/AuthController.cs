using ApiUsuario.Common.Exceptions;
using ApiUsuario.Configuration;
using ApiUsuario.Dtos;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ApiUsuario.Controllers;

/// <summary>
/// Emision de tokens de acceso. Por ahora solo existe el login generico de
/// servicios; mas adelante se agregara el login de usuarios.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(
    ITokenService tokenService,
    IOptions<SecurityOptions> securityOptions) : ControllerBase
{
    /// <summary>Genera un access token JWT generico.</summary>
    /// <remarks>
    /// No requiere body. Envie la API key en la cabecera 'X-API-KEY'; debe ser
    /// el valor configurado en 'Security:ApiKey' (o la variable de entorno
    /// 'API_KEY', rol client) o en 'Security:AdminApiKey' (o 'ADMIN_API_KEY',
    /// rol admin). El token devuelto debe usarse en el resto de endpoints como
    /// 'Authorization: Bearer {accessToken}'. La API key tambien debe seguir
    /// enviandose en 'X-API-KEY' en cada peticion protegida.
    /// </remarks>
    [HttpPost("token")]
    [AllowAnonymous]
    [EnableRateLimiting("sensitive")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public ActionResult<TokenResponseDto> CreateToken()
    {
        var isAdmin = ResolveIsAdmin(securityOptions.Value);
        var token = tokenService.CreateAccessToken(isAdmin);

        return Ok(new TokenResponseDto
        {
            AccessToken = token.AccessToken,
            TokenType = token.TokenType,
            ExpiresIn = token.ExpiresIn,
            ExpiresAt = token.ExpiresAt
        });
    }

    private bool ResolveIsAdmin(SecurityOptions security)
    {
        // En desarrollo se puede desactivar la API key; en ese caso se mantiene
        // el mismo comportamiento que el handler anterior (acceso administrativo).
        if (!security.IsApiKeyRequired())
        {
            return true;
        }

        if (HttpContext.Items.TryGetValue(ApiKeyDefaults.IsAdminItemKey, out var value)
            && value is bool isAdmin)
        {
            return isAdmin;
        }

        throw new UnauthorizedException("Falta la cabecera X-API-KEY o su valor no es valido.");
    }
}
