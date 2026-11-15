using ApiUsuario.Common.Errors;
using ApiUsuario.Common.Http;
using ApiUsuario.Configuration;
using ApiUsuario.Security;
using Microsoft.Extensions.Options;

namespace ApiUsuario.Middleware;

/// <summary>
/// Exige la cabecera 'X-API-KEY' en todas las rutas protegidas. La API key se
/// compara con 'Security:ApiKey' (o la variable de entorno 'API_KEY') y con
/// 'Security:AdminApiKey' (o 'ADMIN_API_KEY'). '/', '/health', Swagger y las
/// peticiones CORS preflight quedan excluidos.
/// </summary>
public sealed partial class ApiKeyMiddleware(
    RequestDelegate next,
    IOptions<SecurityOptions> securityOptions,
    ILogger<ApiKeyMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var security = securityOptions.Value;

        if (!security.IsApiKeyRequired() || IsExcluded(context))
        {
            await next(context);
            return;
        }

        var provided = context.Request.Headers[ApiKeyDefaults.HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(provided))
        {
            LogRejectedApiKey(
                logger,
                ErrorCodes.ApiKeyMissing,
                context.Request.Method,
                context.Request.Path.ToString());

            await WriteUnauthorizedAsync(
                context,
                ErrorCodes.ApiKeyMissing,
                "Falta la cabecera X-API-KEY. Enviela con el valor de la API key configurada.");

            return;
        }

        if (SecretComparer.Matches(security.AdminApiKey, provided))
        {
            context.Items[ApiKeyDefaults.IsAdminItemKey] = true;
        }
        else if (SecretComparer.Matches(security.ApiKey, provided))
        {
            // Compatibilidad: si no hay llave administrativa separada, la
            // principal conserva permisos de administracion.
            context.Items[ApiKeyDefaults.IsAdminItemKey] =
                string.IsNullOrWhiteSpace(security.AdminApiKey);
        }
        else
        {
            LogRejectedApiKey(
                logger,
                ErrorCodes.ApiKeyInvalid,
                context.Request.Method,
                context.Request.Path.ToString());

            await WriteUnauthorizedAsync(
                context,
                ErrorCodes.ApiKeyInvalid,
                "La API key enviada en X-API-KEY no es valida.");

            return;
        }

        await next(context);
    }

    private static bool IsExcluded(HttpContext context)
    {
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            return true;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        return path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/health", StringComparison.OrdinalIgnoreCase)
            || path == "/";
    }

    private static Task WriteUnauthorizedAsync(
        HttpContext context,
        string code,
        string detail) =>
        ProblemResponse.WriteAsync(
            context,
            StatusCodes.Status401Unauthorized,
            code,
            "No autorizado",
            detail,
            cancellationToken: context.RequestAborted);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Solicitud rechazada por API key ({Code}) en {Method} {Path}")]
    private static partial void LogRejectedApiKey(
        ILogger logger,
        string code,
        string method,
        string path);
}
