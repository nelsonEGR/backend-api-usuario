using System.Security.Claims;
using System.Text;
using ApiUsuario.Common.Errors;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiUsuario.Tests.Security;

public sealed class JsonAuthorizationResultHandlerTests
{
    [Fact]
    public async Task SinAccessToken_Devuelve401ConMotivo()
    {
        var context = CreateContext();

        await InvokeAsync(context, PolicyAuthorizationResult.Challenge());

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Contains("application/problem+json", context.Response.ContentType);

        var body = await ReadBodyAsync(context);

        Assert.Contains(ErrorCodes.TokenMissing, body);
        Assert.Contains("Authorization: Bearer", body);
        Assert.Contains("traceId", body);
    }

    [Fact]
    public async Task AccessTokenExpirado_Devuelve401ConMotivo()
    {
        var context = CreateContext();
        context.Request.Headers.Authorization = "Bearer token-vencido";
        AuthFailureMessages.Set(
            context,
            ErrorCodes.TokenExpired,
            "El access token expiro. Solicite uno nuevo.");

        await InvokeAsync(context, PolicyAuthorizationResult.Challenge());

        var body = await ReadBodyAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Contains(ErrorCodes.TokenExpired, body);
        Assert.Contains("expiro", body);
    }

    [Fact]
    public async Task CabeceraConEsquemaIncorrecto_Devuelve401ConMotivo()
    {
        var context = CreateContext();
        context.Request.Headers.Authorization = "Basic abc";

        await InvokeAsync(context, PolicyAuthorizationResult.Challenge());

        var body = await ReadBodyAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Contains(ErrorCodes.TokenMalformed, body);
        Assert.Contains("Bearer", body);
    }

    [Fact]
    public async Task AccessTokenSinRolAdmin_Devuelve403ConMotivo()
    {
        var context = CreateContext();
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Role, "client")], "TestAuth"));

        await InvokeAsync(context, PolicyAuthorizationResult.Forbid());

        var body = await ReadBodyAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Contains(ErrorCodes.ForbiddenRole, body);
        Assert.Contains("rol", body);
    }

    [Fact]
    public async Task AutorizacionCorrecta_LlamaSiguiente()
    {
        var context = CreateContext();
        var nextCalled = false;

        var handler = new JsonAuthorizationResultHandler(
            NullLogger<JsonAuthorizationResultHandler>.Instance);
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        await handler.HandleAsync(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            context,
            policy,
            PolicyAuthorizationResult.Success());

        Assert.True(nextCalled);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/usuarios";
        context.TraceIdentifier = "trace-test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task InvokeAsync(
        DefaultHttpContext context,
        PolicyAuthorizationResult result)
    {
        var handler = new JsonAuthorizationResultHandler(
            NullLogger<JsonAuthorizationResultHandler>.Instance);

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        await handler.HandleAsync(
            _ => Task.CompletedTask,
            context,
            policy,
            result);
    }

    private static async Task<string> ReadBodyAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;

        using var reader = new StreamReader(
            context.Response.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        return await reader.ReadToEndAsync();
    }
}
