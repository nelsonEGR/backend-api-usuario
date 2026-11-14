using ApiUsuario.Common.Exceptions;
using ApiUsuario.Configuration;
using ApiUsuario.Dtos;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    /// Envie el secreto configurado en 'Security:ApiKey' (rol client) o en
    /// 'Security:AdminApiKey' (rol admin). El token devuelto debe usarse en el
    /// resto de endpoints como 'Authorization: Bearer {accessToken}'.
    /// </remarks>
    /// <param name="request">Credenciales del cliente de servicio.</param>
    [HttpPost("token")]
    [AllowAnonymous]
    [EnableRateLimiting("sensitive")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public ActionResult<TokenResponseDto> CreateToken(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] TokenRequestDto? request)
    {
        var security = securityOptions.Value;
        var clientId = string.IsNullOrWhiteSpace(request?.ClientId)
            ? "generic-client"
            : request.ClientId.Trim();

        if (clientId.Length > 100)
        {
            throw new BadRequestException("El 'clientId' no puede superar 100 caracteres.");
        }

        var isAdmin = Authenticate(request?.ClientSecret, security);
        var token = tokenService.CreateAccessToken(clientId, isAdmin);

        return Ok(new TokenResponseDto
        {
            AccessToken = token.AccessToken,
            TokenType = token.TokenType,
            ExpiresIn = token.ExpiresIn,
            ExpiresAt = token.ExpiresAt
        });
    }

    private static bool Authenticate(string? providedSecret, SecurityOptions security)
    {
        // En desarrollo se puede desactivar la API key; en ese caso se mantiene
        // el mismo comportamiento que el handler anterior (acceso administrativo).
        if (!security.IsApiKeyRequired())
        {
            return true;
        }

        if (SecretComparer.Matches(security.AdminApiKey, providedSecret))
        {
            return true;
        }

        if (SecretComparer.Matches(security.ApiKey, providedSecret))
        {
            // Compatibilidad: si no hay llave administrativa separada, la
            // principal conserva permisos de administracion.
            return string.IsNullOrWhiteSpace(security.AdminApiKey);
        }

        throw new UnauthorizedException("Credenciales invalidas. Verifique el clientSecret.");
    }
}
