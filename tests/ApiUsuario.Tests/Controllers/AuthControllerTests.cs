using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiUsuario.Common.Exceptions;
using ApiUsuario.Configuration;
using ApiUsuario.Controllers;
using ApiUsuario.Dtos;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiUsuario.Tests.Controllers;

public sealed class AuthControllerTests
{
    private const string Key = "clave-de-prueba-suficientemente-larga-1234567890";

    [Fact]
    public void CreateToken_ConSecretoCliente_DevuelveTokenRolClient()
    {
        var controller = CreateController(
            new SecurityOptions
            {
                ApiKey = "client-key",
                AdminApiKey = "admin-key",
                RequireApiKey = true
            });

        var result = controller.CreateToken(new TokenRequestDto { ClientSecret = "client-key" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(ok.Value);
        var principal = Validate(response.AccessToken);

        Assert.True(principal.IsInRole("client"));
        Assert.Equal("generic-client", principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void CreateToken_ConSecretoAdmin_DevuelveTokenRolAdmin()
    {
        var controller = CreateController(
            new SecurityOptions
            {
                ApiKey = "client-key",
                AdminApiKey = "admin-key",
                RequireApiKey = true
            });

        var result = controller.CreateToken(
            new TokenRequestDto { ClientId = "servicio", ClientSecret = "admin-key" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(ok.Value);

        Assert.True(Validate(response.AccessToken).IsInRole("admin"));
    }

    [Fact]
    public void CreateToken_ConSecretoInvalido_LanzaUnauthorized()
    {
        var controller = CreateController(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true });

        Assert.Throws<UnauthorizedException>(() =>
            controller.CreateToken(new TokenRequestDto { ClientSecret = "otro" }));
    }

    [Fact]
    public void CreateToken_SinApiKeyRequerida_EmiteTokenAdmin()
    {
        var controller = CreateController(new SecurityOptions { RequireApiKey = false });

        var result = controller.CreateToken(null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(ok.Value);

        Assert.True(Validate(response.AccessToken).IsInRole("admin"));
    }

    [Fact]
    public void CreateToken_ClientIdMuyLargo_LanzaBadRequest()
    {
        var controller = CreateController(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true });

        Assert.Throws<BadRequestException>(() =>
            controller.CreateToken(new TokenRequestDto
            {
                ClientId = new string('a', 101),
                ClientSecret = "client-key"
            }));
    }

    private static AuthController CreateController(SecurityOptions security)
    {
        var tokenService = new JwtTokenService(
            Options.Create(new JwtOptions
            {
                Key = Key,
                Issuer = "api-usuario",
                Audience = "api-usuario-clients",
                ExpiresMinutes = 5
            }),
            TimeProvider.System);

        return new AuthController(tokenService, Options.Create(security));
    }

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
