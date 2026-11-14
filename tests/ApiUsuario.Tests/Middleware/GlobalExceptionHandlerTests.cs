using System.Text;
using ApiUsuario.Common.Exceptions;
using ApiUsuario.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

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
        Assert.Contains("No existe el usuario.", body);
    }

    [Fact]
    public async Task TryHandleAsync_ConflictException_DevuelveProblemJson409()
    {
        var (handled, status, _, body) = await HandleAsync(
            new ConflictException("Ya existe un usuario con ese email."));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, status);
        Assert.Contains("Ya existe un usuario con ese email.", body);
    }

    [Fact]
    public async Task TryHandleAsync_ExcepcionNoControlada_Devuelve500SinFiltrarDetalles()
    {
        var (handled, status, _, body) = await HandleAsync(
            new InvalidOperationException("Detalle interno sensible."));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, status);
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
