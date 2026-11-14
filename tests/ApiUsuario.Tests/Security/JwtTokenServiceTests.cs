using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiUsuario.Configuration;
using ApiUsuario.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiUsuario.Tests.Security;

public sealed class JwtTokenServiceTests
{
    private const string Key = "clave-de-prueba-suficientemente-larga-1234567890";

    [Fact]
    public void CreateAccessToken_DevuelveTokenValidoConRolClient()
    {
        var service = CreateService();

        var result = service.CreateAccessToken(isAdmin: false);
        var principal = Validate(result.AccessToken);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal("generic-client", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.True(principal.IsInRole("client"));
        Assert.Equal(300, result.ExpiresIn);
    }

    [Fact]
    public void CreateAccessToken_ConAdmin_DevuelveRolAdmin()
    {
        var service = CreateService();

        var result = service.CreateAccessToken(isAdmin: true);
        var principal = Validate(result.AccessToken);

        Assert.True(principal.IsInRole("admin"));
    }

    [Fact]
    public void CreateAccessToken_ExpiraEnCincoMinutos()
    {
        var service = CreateService();
        var before = DateTimeOffset.UtcNow;

        var result = service.CreateAccessToken(isAdmin: false);

        Assert.InRange(
            result.ExpiresAt,
            before.AddMinutes(5).AddSeconds(-2),
            before.AddMinutes(5).AddSeconds(2));
    }

    private static JwtTokenService CreateService() =>
        new(
            Options.Create(new JwtOptions
            {
                Key = Key,
                Issuer = "api-usuario",
                Audience = "api-usuario-clients",
                ExpiresMinutes = 5
            }),
            TimeProvider.System);

    private static ClaimsPrincipal Validate(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        return handler.ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "api-usuario",
                ValidateAudience = true,
                ValidAudience = "api-usuario-clients",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(5),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            },
            out _);
    }
}
