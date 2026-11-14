using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiUsuario.Swagger;

/// <summary>
/// Marca cada operacion con doble validacion (Bearer + X-API-KEY) salvo las
/// que son anonimas (por ejemplo, el login generico). Asi Swagger envia ambos
/// headers en las peticiones protegidas.
/// </summary>
public sealed class SecurityRequirementsOperationFilter : IOperationFilter
{
    private const string BearerSchemeId = "Bearer";
    private const string ApiKeySchemeId = "ApiKey";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IsAnonymous(context.MethodInfo))
        {
            operation.Security.Clear();
            return;
        }

        operation.Security =
        [
            Requirement(BearerSchemeId),
            Requirement(ApiKeySchemeId)
        ];
    }

    private static OpenApiSecurityRequirement Requirement(string schemeId) =>
        new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = schemeId
                    }
                },
                Array.Empty<string>()
            }
        };

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
