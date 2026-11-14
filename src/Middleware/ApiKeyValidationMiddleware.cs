using ApiUsuario.Configuration;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ApiUsuario.Middleware;

/// <summary>
/// Doble validacion: ademas del access token JWT, exige el header X-API-KEY
/// en todos los endpoints protegidos. Los endpoints anonimos (login generico,
/// health, raiz y Swagger) quedan exentos.
/// </summary>
public sealed class ApiKeyValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is not null && !AllowsAnonymous(endpoint))
        {
            var security = context.RequestServices
                .GetRequiredService<IOptionsMonitor<SecurityOptions>>()
                .CurrentValue;

            if (security.IsApiKeyRequired() && !IsValidApiKey(context, security))
            {
                await WriteProblemAsync(context);
                return;
            }
        }

        await next(context);
    }

    private static bool AllowsAnonymous(Endpoint endpoint) =>
        endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;

    private static bool IsValidApiKey(HttpContext context, SecurityOptions security)
    {
        var providedKey = context.Request.Headers["X-API-KEY"].ToString();

        return SecretComparer.Matches(security.AdminApiKey, providedKey)
            || SecretComparer.Matches(security.ApiKey, providedKey);
    }

    private static async Task WriteProblemAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        const string detail =
            "La doble validacion fallo: envie el access token (Authorization: Bearer) " +
            "y la API key (X-API-KEY) en la misma peticion.";

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "No autorizado",
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["mensaje"] = detail;

        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }
}
