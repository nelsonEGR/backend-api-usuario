using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiUsuario.Swagger;

/// <summary>
/// Marca cada operacion con los esquemas de seguridad que exige. Las rutas
/// protegidas requieren 'Authorization: Bearer {token}' y la cabecera
/// 'X-API-KEY'. El login generico solo requiere la cabecera 'X-API-KEY'.
/// </summary>
public sealed class SecurityRequirementsOperationFilter : IOperationFilter
{
    private const string BearerSchemeId = "Bearer";
    private const string ApiKeySchemeId = "ApiKey";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IsAnonymous(context.MethodInfo))
        {
            operation.Security = [Requirement(ApiKeySchemeId)];
            return;
        }

        // Ambos esquemas van en el mismo requerimiento para exigirlos juntos
        // (token + API key), no como alternativas.
        operation.Security =
        [
            Requirement(BearerSchemeId, ApiKeySchemeId)
        ];
    }

    private static OpenApiSecurityRequirement Requirement(params string[] schemeIds)
    {
        var requirement = new OpenApiSecurityRequirement();

        foreach (var schemeId in schemeIds)
        {
            requirement.Add(
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = schemeId
                    }
                },
                Array.Empty<string>());
        }

        return requirement;
    }

    private static bool IsAnonymous(MethodInfo method)
    {
        var hasMethodAttribute = method
            .GetCustomAttributes(inherit: true)
            .OfType<AllowAnonymousAttribute>()
            .Any();

        var hasControllerAttribute = method.DeclaringType?
            .GetCustomAttributes(inherit: true)
            .OfType<AllowAnonymousAttribute>()
            .Any() ?? false;

        return hasMethodAttribute || hasControllerAttribute;
    }
}
