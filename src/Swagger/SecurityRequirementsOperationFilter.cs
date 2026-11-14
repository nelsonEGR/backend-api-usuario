using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiUsuario.Swagger;

/// <summary>
/// Marca cada operacion protegida con el esquema Bearer. Las operaciones
/// anonimas (por ejemplo, el login generico) quedan sin requisitos de
/// seguridad. La API key no se envia como header: solo se usa como
/// 'clientSecret' al solicitar el token.
/// </summary>
public sealed class SecurityRequirementsOperationFilter : IOperationFilter
{
    private const string BearerSchemeId = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IsAnonymous(context.MethodInfo))
        {
            operation.Security.Clear();
            return;
        }

        operation.Security = [Requirement(BearerSchemeId)];
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
