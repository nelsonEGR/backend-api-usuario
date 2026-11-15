using System.Data.Common;
using System.Text.Json;
using ApiUsuario.Common.Errors;
using ApiUsuario.Common.Exceptions;
using ApiUsuario.Common.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiUsuario.Middleware;

/// <summary>
/// Traduce excepciones a ProblemDetails sin filtrar detalles internos de la base
/// de datos. Incluye las propiedades "codigo" y "mensaje" para que el cliente
/// sepa por que fallo la solicitud y para poder buscarla en los logs.
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

        var (statusCode, code, title, detail) = Map(exception, httpContext.Request.Method);

        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.ToString();

        if (statusCode == StatusCodes.Status204NoContent)
        {
            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            return true;
        }

        if (statusCode >= 500)
        {
            LogUnhandledError(logger, exception, code, method, path);
        }
        else
        {
            LogHandledError(logger, statusCode, code, method, path, detail);
        }

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        await ProblemResponse.WriteAsync(
            httpContext,
            statusCode,
            code,
            title,
            detail,
            cancellationToken: cancellationToken);

        return true;
    }

    private static (int Status, string Code, string Title, string Detail) Map(
        Exception exception,
        string method) =>
        exception switch
        {
            BadRequestException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.BadRequest,
                "Solicitud invalida",
                exception.Message),
            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                ErrorCodes.Unauthorized,
                "No autorizado",
                exception.Message),
            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                ErrorCodes.Forbidden,
                "Acceso denegado",
                exception.Message),
            NotFoundException when HttpMethods.IsGet(method) => NoContent(),
            NotFoundException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.ResourceNotFound,
                "Solicitud invalida",
                exception.Message),
            KeyNotFoundException when HttpMethods.IsGet(method) => NoContent(),
            KeyNotFoundException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.ResourceNotFound,
                "Solicitud invalida",
                "El recurso solicitado no existe."),
            ConflictException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.Conflict,
                "Solicitud invalida",
                exception.Message),
            FormatException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.BadRequest,
                "Solicitud invalida",
                "El formato de uno de los datos enviados no es valido."),
            OverflowException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.BadRequest,
                "Solicitud invalida",
                "Uno de los valores enviados esta fuera del rango permitido."),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.BadRequest,
                "Solicitud invalida",
                "Los datos enviados no son validos."),
            NotSupportedException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.BadRequest,
                "Solicitud invalida",
                "La operacion solicitada no es soportada."),
            BadHttpRequestException badRequest => FromStatus(
                badRequest.StatusCode is >= 400 and <= 499
                    ? badRequest.StatusCode
                    : StatusCodes.Status400BadRequest),
            JsonException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.BadRequest,
                "Solicitud invalida",
                "El cuerpo JSON no es valido. Verifique la sintaxis del cuerpo enviado."),
            OperationCanceledException => (
                StatusCodes.Status504GatewayTimeout,
                ErrorCodes.GatewayTimeout,
                "Tiempo de espera agotado",
                "La operacion tardo demasiado. Intente nuevamente."),
            TimeoutException => (
                StatusCodes.Status504GatewayTimeout,
                ErrorCodes.GatewayTimeout,
                "Tiempo de espera agotado",
                "La operacion tardo demasiado. Intente nuevamente."),
            DbUpdateConcurrencyException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.Conflict,
                "Solicitud invalida",
                "El registro fue modificado por otra operacion. Recargue los datos e intente nuevamente."),
            DbUpdateException updateException => MapDbUpdate(updateException),
            PostgresException postgresException => MapPostgres(postgresException),
            NpgsqlException => DatabaseUnavailable(),
            DbException => DatabaseUnavailable(),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                ErrorCodes.Forbidden,
                "Acceso denegado",
                "No tiene permisos para realizar esta operacion."),
            _ => MapByInnerException(exception)
        };

    private static (int Status, string Code, string Title, string Detail) NoContent() => (
        StatusCodes.Status204NoContent,
        "NO_CONTENT",
        "Sin contenido",
        string.Empty);

    private static (int Status, string Code, string Title, string Detail) MapByInnerException(
        Exception exception)
    {
        for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is PostgresException postgresException)
            {
                return MapPostgres(postgresException);
            }

            if (inner is NpgsqlException or DbException)
            {
                return DatabaseUnavailable();
            }
        }

        return (
            StatusCodes.Status500InternalServerError,
            ErrorCodes.InternalError,
            "Error interno",
            "Ocurrio un error inesperado. Intente nuevamente o contacte al administrador.");
    }

    private static (int Status, string Code, string Title, string Detail) FromStatus(int statusCode)
    {
        var (code, title, detail) = StatusCodeProblemCatalog.Describe(statusCode);
        return (ClientErrorStatus.Normalize(statusCode), code, title, detail);
    }

    private static (int Status, string Code, string Title, string Detail) MapDbUpdate(
        DbUpdateException exception)
    {
        if (exception.InnerException is PostgresException postgresException)
        {
            return MapPostgres(postgresException);
        }

        return (
            StatusCodes.Status503ServiceUnavailable,
            ErrorCodes.ServiceUnavailable,
            "Servicio no disponible",
            "No fue posible guardar los cambios en la base de datos. Intente nuevamente.");
    }

    private static (int Status, string Code, string Title, string Detail) MapPostgres(
        PostgresException exception)
    {
        if (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return (
                StatusCodes.Status400BadRequest,
                ErrorCodes.Conflict,
                "Solicitud invalida",
                "Ya existe un registro con los mismos datos.");
        }

        if (exception.SqlState.StartsWith("23", StringComparison.Ordinal))
        {
            return (
                StatusCodes.Status400BadRequest,
                ErrorCodes.Conflict,
                "Solicitud invalida",
                "Los datos enviados entran en conflicto con la informacion existente.");
        }

        return DatabaseUnavailable();
    }

    private static (int Status, string Code, string Title, string Detail) DatabaseUnavailable() => (
        StatusCodes.Status503ServiceUnavailable,
        ErrorCodes.ServiceUnavailable,
        "Servicio no disponible",
        "La base de datos no esta disponible en este momento. Intente nuevamente.");

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Error,
        Message = "Error no controlado ({Code}) en {Method} {Path}")]
    private static partial void LogUnhandledError(
        ILogger logger,
        Exception exception,
        string code,
        string method,
        string path);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "{StatusCode} ({Code}) en {Method} {Path}: {Message}")]
    private static partial void LogHandledError(
        ILogger logger,
        int statusCode,
        string code,
        string method,
        string path,
        string message);
}
