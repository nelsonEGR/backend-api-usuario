using ApiUsuario.Controllers;
using ApiUsuario.Dtos;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiUsuario.Swagger;

/// <summary>
/// GET /api/Usuarios responde con un arreglo cuando no se envia 'id' y con un
/// objeto cuando se envia. Este filtro lo documenta como 'oneOf' para que
/// Swagger no muestre un unico esquema enganoso.
/// </summary>
public sealed class UsuariosGetResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(UsuariosController)
            || context.MethodInfo.Name != nameof(UsuariosController.GetUsuarios))
        {
            return;
        }

        if (!operation.Responses.TryGetValue("200", out var response)
            || !response.Content.TryGetValue("application/json", out var mediaType))
        {
            return;
        }

        mediaType.Schema = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = nameof(UsuarioDto)
                        }
                    }
                },
                new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = nameof(UsuarioDto)
                    }
                }
            ]
        };
    }
}
