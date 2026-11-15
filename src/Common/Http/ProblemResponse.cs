using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Common.Http;

/// <summary>
/// Escribe respuestas de error uniformes (application/problem+json) con el
/// codigo del error, el mensaje, el detalle y el identificador de la peticion.
/// </summary>
public static class ProblemResponse
{
    public static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string code,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null,
        CancellationToken cancellationToken = default)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentLength = null;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["codigo"] = code;
        problem.Extensions["mensaje"] = detail;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (errors is { Count: > 0 })
        {
            problem.Extensions["errores"] = errors;
        }

        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
    }
}
