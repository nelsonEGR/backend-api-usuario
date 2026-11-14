using ApiUsuario.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Middleware;

/// <summary>
/// Traduce excepciones a ProblemDetails sin filtrar detalles internos de la base de datos.
/// Incluye la propiedad "mensaje" por compatibilidad con los clientes existentes.
/// </summary>
public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerro la conexion; no hay a quien responder.
            return true;
        }

        var (statusCode, title, detail) = exception switch
        {
            BadRequestException => (
                StatusCodes.Status400BadRequest,
                "Solicitud invalida",
                exception.Message),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                exception.Message),
            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflicto",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "Ocurrio un error inesperado. Intente nuevamente o contacte al administrador.")
        };

        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.ToString();

        if (statusCode >= 500)
        {
            LogUnhandledError(logger, exception, method, path);
        }
        else
        {
            LogHandledError(logger, statusCode, method, path, exception.Message);
        }

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["mensaje"] = detail;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Error,
        Message = "Error no controlado en {Method} {Path}")]
    private static partial void LogUnhandledError(
        ILogger logger,
        Exception exception,
        string method,
        string path);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "{StatusCode} en {Method} {Path}: {Message}")]
    private static partial void LogHandledError(
        ILogger logger,
        int statusCode,
        string method,
        string path,
        string message);
}
