using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiUsuario.Swagger;

/// <summary>
/// Elimina las definiciones de modelos del documento OpenAPI para no exponer
/// la estructura interna de los DTOs. Swagger conserva rutas, metodos y
/// parametros, que es lo necesario para generar las URLs/curl.
/// </summary>
public sealed class HideSchemasDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Components ??= new OpenApiComponents();
        swaggerDoc.Components.Schemas?.Clear();

        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations.Values)
            {
                ReplaceRequestSchemas(operation);
                ReplaceResponseSchemas(operation);
                ReplaceParameterSchemas(operation);
            }
        }
    }

    private static void ReplaceRequestSchemas(OpenApiOperation operation)
    {
        if (operation.RequestBody is null)
        {
            return;
        }

        foreach (var mediaType in operation.RequestBody.Content.Values)
        {
            mediaType.Schema = GenericObject();
        }
    }

    private static void ReplaceResponseSchemas(OpenApiOperation operation)
    {
        foreach (var response in operation.Responses.Values)
        {
            foreach (var mediaType in response.Content.Values)
            {
                mediaType.Schema = GenericObject();
            }
        }
    }

    private static void ReplaceParameterSchemas(OpenApiOperation operation)
    {
        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Schema?.Reference is not null)
            {
                parameter.Schema = GenericObject();
            }
        }
    }

    private static OpenApiSchema GenericObject() => new() { Type = "object" };
}
