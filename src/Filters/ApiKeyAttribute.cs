using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TuProyecto.Filters
{
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Detectar si estamos en Render
            var isRender = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RENDER"));

            if (isRender)
            {
                // Solo validar API key en Render
                if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
                {
                    context.Result = new UnauthorizedObjectResult(new { mensaje = "Falta API Key" });
                    return;
                }

                var apiKey = Environment.GetEnvironmentVariable("API_KEY");

                if (string.IsNullOrEmpty(apiKey) || !apiKey.Equals(extractedApiKey))
                {
                    context.Result = new UnauthorizedObjectResult(new { mensaje = "API Key inválida" });
                    return;
                }
            }

            await next();
        }
    }
}