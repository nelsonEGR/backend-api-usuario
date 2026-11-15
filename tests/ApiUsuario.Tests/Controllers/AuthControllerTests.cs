using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiUsuario.Common.Exceptions;
using ApiUsuario.Configuration;
using ApiUsuario.Controllers;
using ApiUsuario.Dtos;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiUsuario.Tests.Controllers;

public sealed class AuthControllerTests
{
    private const string Key = "clave-de-prueba-suficientemente-larga-1234567890";

    [Fact]
    public void CreateToken_ConApiKeyCliente_DevuelveTokenRolClient()
    {
        var controller = CreateController(
            new SecurityOptions
            {
                ApiKey = "client-key",
                AdminApiKey = "admin-key",
                RequireApiKey = true
            },
            isAdmin: false);

        var result = controller.CreateToken();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(ok.Value);
        var principal = Validate(response.AccessToken);

        Assert.True(principal.IsInRole("client"));
        Assert.Equal("generic-client", principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void CreateToken_ConApiKeyAdmin_DevuelveTokenRolAdmin()
    {
        var controller = CreateController(
            new SecurityOptions
            {
                ApiKey = "client-key",
                AdminApiKey = "admin-key",
                RequireApiKey = true
            },
            isAdmin: true);

        var result = controller.CreateToken();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(ok.Value);

        Assert.True(Validate(response.AccessToken).IsInRole("admin"));
    }

    [Fact]
    public void CreateToken_SinApiKeyValidadaEnElContexto_LanzaUnauthorized()
    {
        var controller = CreateController(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true });

        Assert.Throws<UnauthorizedException>(() => controller.CreateToken());
    }

    [Fact]
    public void CreateToken_SinApiKeyRequerida_EmiteTokenAdmin()
    {
        var controller = CreateController(new SecurityOptions { RequireApiKey = false });

        var result = controller.CreateToken();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(ok.Value);

        Assert.True(Validate(response.AccessToken).IsInRole("admin"));
    }

    private static AuthController CreateController(SecurityOptions security, bool? isAdmin = null)
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

        var httpContext = new DefaultHttpContext();

        if (isAdmin.HasValue)
        {
            httpContext.Items[ApiKeyDefaults.IsAdminItemKey] = isAdmin.Value;
        }

        return new AuthController(tokenService, Options.Create(security))
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
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
