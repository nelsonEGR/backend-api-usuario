using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using ApiUsuario.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ApiUsuario.Security;

/// <summary>
/// Autenticacion por API key. La validacion se activa por configuracion
/// (Security:RequireApiKey) y nunca depende del proveedor de hosting.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly SecurityOptions _security;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptionsMonitor<SecurityOptions> security)
        : base(options, logger, encoder)
    {
        _security = security.CurrentValue;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_security.IsApiKeyRequired())
        {
            return Task.FromResult(AuthenticateResult.Success(CreateTicket("development", isAdmin: true)));
        }

        if (string.IsNullOrWhiteSpace(_security.ApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail(
                "La API key no esta configurada en el servidor."));
        }

        if (!Request.Headers.TryGetValue(Options.HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedKey = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(providedKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("La API key esta vacia."));
        }

        if (Matches(_security.AdminApiKey, providedKey))
        {
            return Task.FromResult(AuthenticateResult.Success(CreateTicket("admin", isAdmin: true)));
        }

        if (Matches(_security.ApiKey, providedKey))
        {
            // Compatibilidad: si no existe una llave administrativa separada,
            // la llave principal conserva los permisos de administracion.
            var actsAsAdmin = string.IsNullOrWhiteSpace(_security.AdminApiKey);
            return Task.FromResult(AuthenticateResult.Success(
                CreateTicket(actsAsAdmin ? "admin" : "client", actsAsAdmin)));
        }

        return Task.FromResult(AuthenticateResult.Fail("API Key invalida o no autorizada."));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        await WriteProblemAsync(
            StatusCodes.Status401Unauthorized,
            "No autorizado",
            "Falta la API Key o no es valida. Envie el header X-API-KEY.");
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        await WriteProblemAsync(
            StatusCodes.Status403Forbidden,
            "Acceso denegado",
            "La API Key es valida, pero no tiene permisos para este recurso.");
    }

    private AuthenticationTicket CreateTicket(string clientType, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "api-client"),
            new(ClaimTypes.Name, "api-client"),
            new(ClaimTypes.Role, isAdmin ? "admin" : "client"),
            new("client_type", clientType)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
    }

    private static bool Matches(string? expected, string provided)
    {
        if (string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(provided));

        return CryptographicOperations.FixedTimeEquals(expectedHash, providedHash);
    }

    private async Task WriteProblemAsync(int statusCode, string title, string detail)
    {
        if (Response.HasStarted)
        {
            return;
        }

        Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = Request.Path
        };
        problem.Extensions["mensaje"] = detail;

        await Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: Context.RequestAborted);
    }
}
