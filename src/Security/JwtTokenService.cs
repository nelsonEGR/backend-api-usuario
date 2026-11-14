using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiUsuario.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiUsuario.Security;

/// <summary>
/// Genera access tokens JWT firmados con HMAC-SHA256. El token es generico:
/// identifica un cliente de servicio, no a un usuario final.
/// </summary>
public sealed class JwtTokenService(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : ITokenService
{
    private const string ClientId = "generic-client";

    private static readonly JwtSecurityTokenHandler Handler = new();

    private readonly JwtOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    public TokenResult CreateAccessToken(bool isAdmin)
    {
        var now = _timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, ClientId),
            new(ClaimTypes.NameIdentifier, ClientId),
            new(ClaimTypes.Name, ClientId),
            new(ClaimTypes.Role, isAdmin ? "admin" : "client"),
            new("client_type", isAdmin ? "admin" : "generic"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = Handler.WriteToken(token);

        return new TokenResult(
            accessToken,
            "Bearer",
            (int)Math.Round((expiresAt - now).TotalSeconds),
            expiresAt);
    }
}
