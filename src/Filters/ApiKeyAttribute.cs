using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TuProyecto.Filters
{
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Detectar si estamos en Render (variable RENDER = "true")
            var isRender = string.Equals(
                Environment.GetEnvironmentVariable("RENDER"),
                "true",
                StringComparison.OrdinalIgnoreCase
            );

            if (isRender)
            {
                // Validar que el header exista
                if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
                {
                    context.Result = new UnauthorizedObjectResult(new { mensaje = "Falta API Key en el header X-API-KEY" });
                    return;
                }

                // Leer la API key desde variables de entorno
                var apiKey = Environment.GetEnvironmentVariable("API_KEY");

                if (string.IsNullOrWhiteSpace(apiKey) ||
                    !string.Equals(apiKey, extractedApiKey, StringComparison.Ordinal))
                {
                    context.Result = new UnauthorizedObjectResult(new { mensaje = "API Key inválida o no autorizada" });
                    return;
                }
            }

            await next();
        }
    }
}