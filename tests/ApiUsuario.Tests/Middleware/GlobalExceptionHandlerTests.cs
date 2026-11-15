using System.Text;
using System.Text.Json;
using ApiUsuario.Common.Exceptions;
using ApiUsuario.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace ApiUsuario.Tests.Middleware;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_NotFoundException_DevuelveProblemJson404()
    {
        var (handled, status, contentType, body) = await HandleAsync(
            new NotFoundException("No existe el usuario."));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, status);
        Assert.Contains("application/problem+json", contentType);
        Assert.Contains("mensaje", body);
        Assert.Contains("codigo", body);
        Assert.Contains("NOT_FOUND", body);
        Assert.Contains("No existe el usuario.", body);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_DevuelveProblemJson409()
    {
        var (handled, status, _, body) = await HandleAsync(
            new ConflictException("Ya existe un usuario con ese email."));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, status);
        Assert.Contains("CONFLICT", body);
        Assert.Contains("Ya existe un usuario con ese email.", body);
    }

    [Fact]
    public async Task TryHandleAsync_ForbiddenException_DevuelveProblemJson403()
    {
        var (handled, status, _, body) = await HandleAsync(
            new ForbiddenException("No tiene permisos."));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status403Forbidden, status);
        Assert.Contains("FORBIDDEN", body);
        Assert.Contains("No tiene permisos.", body);
    }

    [Fact]
    public async Task TryHandleAsync_JsonInvalido_DevuelveProblemJson400()
    {
        var (handled, status, _, body) = await HandleAsync(new JsonException("JSON roto"));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Contains("BAD_REQUEST", body);
        Assert.DoesNotContain("JSON roto", body);
    }

    [Fact]
    public async Task TryHandleAsync_Timeout_DevuelveProblemJson504()
    {
        var (handled, status, _, body) = await HandleAsync(new TimeoutException());

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, status);
        Assert.Contains("GATEWAY_TIMEOUT", body);
    }

    [Fact]
    public async Task TryHandleAsync_BaseDeDatosNoDisponible_DevuelveProblemJson503()
    {
        var (handled, status, _, body) = await HandleAsync(
            new NpgsqlException("connection refused"));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, status);
        Assert.Contains("SERVICE_UNAVAILABLE", body);
        Assert.DoesNotContain("connection refused", body);
    }

    [Fact]
    public async Task TryHandleAsync_ExcepcionEnvueltaDeBaseDeDatos_DevuelveProblemJson503()
    {
        var (handled, status, _, body) = await HandleAsync(
            new InvalidOperationException(
                "error al ejecutar la consulta",
                new NpgsqlException("connection refused")));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, status);
        Assert.Contains("SERVICE_UNAVAILABLE", body);
    }

    [Fact]
    public async Task TryHandleAsync_ExcepcionNoControlada_Devuelve500SinFiltrarDetalles()
    {
        var (handled, status, _, body) = await HandleAsync(
            new InvalidOperationException("Detalle interno sensible."));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, status);
        Assert.Contains("INTERNAL_ERROR", body);
        Assert.DoesNotContain("Detalle interno sensible.", body);
    }

    [Fact]
    public async Task TryHandleAsync_ClienteCancela_NoEscribeRespuesta()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        context.RequestAborted = cts.Token;

        var handler = CreateHandler();
        var handled = await handler.TryHandleAsync(
            context,
            new OperationCanceledException(),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    private static async Task<(bool Handled, int Status, string ContentType, string Body)> HandleAsync(
        Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handler = CreateHandler();
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        var body = await ReadBodyAsync(context.Response);

        return (handled, context.Response.StatusCode, context.Response.ContentType ?? string.Empty, body);
    }

    private static GlobalExceptionHandler CreateHandler() =>
        new(NullLogger<GlobalExceptionHandler>.Instance);

    private static async Task<string> ReadBodyAsync(HttpResponse response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(
            response.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        return await reader.ReadToEndAsync();
    }
}
