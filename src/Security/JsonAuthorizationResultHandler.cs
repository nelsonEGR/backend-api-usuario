using ApiUsuario.Common.Errors;
using ApiUsuario.Common.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace ApiUsuario.Security;

/// <summary>
/// Cuando falta el access token, es invalido o no tiene el rol requerido,
/// devuelve un JSON con el motivo en lugar de una respuesta vacia.
/// </summary>
public sealed partial class JsonAuthorizationResultHandler(
    ILogger<JsonAuthorizationResultHandler> logger)
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            var (code, detail) = AuthFailureMessages.Resolve(context);

            LogAuthError(
                logger,
                StatusCodes.Status401Unauthorized,
                code,
                context.Request.Method,
                context.Request.Path.ToString());

            context.Response.Headers["WWW-Authenticate"] = "Bearer";

            await ProblemResponse.WriteAsync(
                context,
                StatusCodes.Status401Unauthorized,
                code,
                "No autorizado",
                detail,
                cancellationToken: context.RequestAborted);

            return;
        }

        if (authorizeResult.Forbidden)
        {
            var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
            var code = isAuthenticated ? ErrorCodes.ForbiddenRole : ErrorCodes.Forbidden;
            var detail = isAuthenticated
                ? "El access token es valido, pero no tiene el rol necesario para esta operacion."
                : "No tiene permisos para realizar esta operacion.";

            LogAuthError(
                logger,
                StatusCodes.Status403Forbidden,
                code,
                context.Request.Method,
                context.Request.Path.ToString());

            await ProblemResponse.WriteAsync(
                context,
                StatusCodes.Status403Forbidden,
                code,
                "Acceso denegado",
                detail,
                cancellationToken: context.RequestAborted);

            return;
        }

        await _default.HandleAsync(next, context, policy, authorizeResult);
    }

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Error de autenticacion {StatusCode} ({Code}) en {Method} {Path}")]
    private static partial void LogAuthError(
        ILogger logger,
        int statusCode,
        string code,
        string method,
        string path);
}
