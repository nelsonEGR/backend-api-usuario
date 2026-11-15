using System.Text;
using ApiUsuario.Configuration;
using ApiUsuario.Middleware;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ApiUsuario.Tests.Middleware;

public sealed class ApiKeyMiddlewareTests
{
    private static readonly SecurityOptions Security = new()
    {
        ApiKey = "client-key",
        AdminApiKey = "admin-key",
        RequireApiKey = true
    };

    [Fact]
    public async Task RutaProtegida_SinHeader_Devuelve401ConMotivo()
    {
        var result = await InvokeAsync(Security, HttpMethods.Get, "/api/usuarios", apiKey: null);

        Assert.False(result.NextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.Context.Response.StatusCode);
        Assert.Contains("application/problem+json", result.Context.Response.ContentType);

        var body = await ReadBodyAsync(result.Context);

        Assert.Contains("API_KEY_MISSING", body);
        Assert.Contains("X-API-KEY", body);
        Assert.Contains("traceId", body);
    }

    [Fact]
    public async Task RutaProtegida_ConHeaderInvalido_Devuelve401ConMotivo()
    {
        var result = await InvokeAsync(Security, HttpMethods.Get, "/api/usuarios", "otro");

        Assert.False(result.NextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.Context.Response.StatusCode);

        var body = await ReadBodyAsync(result.Context);

        Assert.Contains("API_KEY_INVALID", body);
        Assert.Contains("no es valida", body);
    }

    [Fact]
    public async Task RutaProtegida_ConHeaderCliente_LlamaSiguienteComoCliente()
    {
        var result = await InvokeAsync(Security, HttpMethods.Get, "/api/usuarios", "client-key");

        Assert.True(result.NextCalled);
        Assert.Equal(false, result.Context.Items[ApiKeyDefaults.IsAdminItemKey]);
    }

    [Fact]
    public async Task RutaProtegida_ConHeaderAdmin_LlamaSiguienteComoAdmin()
    {
        var result = await InvokeAsync(Security, HttpMethods.Get, "/api/usuarios", "admin-key");

        Assert.True(result.NextCalled);
        Assert.Equal(true, result.Context.Items[ApiKeyDefaults.IsAdminItemKey]);
    }

    [Fact]
    public async Task Login_SinHeader_Devuelve401()
    {
        var result = await InvokeAsync(Security, HttpMethods.Post, "/api/auth/token", apiKey: null);

        Assert.False(result.NextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.Context.Response.StatusCode);
    }

    [Fact]
    public async Task Login_ConHeader_LlamaSiguiente()
    {
        var result = await InvokeAsync(Security, HttpMethods.Post, "/api/auth/token", "client-key");

        Assert.True(result.NextCalled);
    }

    [Fact]
    public async Task HealthYRaiz_SinHeader_LlamanSiguiente()
    {
        var health = await InvokeAsync(Security, HttpMethods.Get, "/health", apiKey: null);
        var root = await InvokeAsync(Security, HttpMethods.Get, "/", apiKey: null);

        Assert.True(health.NextCalled);
        Assert.True(root.NextCalled);
    }

    [Fact]
    public async Task PreflightCors_SinHeader_LlamaSiguiente()
    {
        var result = await InvokeAsync(
            Security,
            HttpMethods.Options,
            "/api/usuarios",
            apiKey: null);

        Assert.True(result.NextCalled);
    }

    [Fact]
    public async Task SeguridadDesactivada_SinHeader_LlamaSiguiente()
    {
        var result = await InvokeAsync(
            new SecurityOptions { RequireApiKey = false },
            HttpMethods.Get,
            "/api/usuarios",
            apiKey: null);

        Assert.True(result.NextCalled);
    }

    [Fact]
    public async Task SinLlaveAdminSeparada_LaLlavePrincipalEsAdmin()
    {
        var result = await InvokeAsync(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true },
            HttpMethods.Get,
            "/api/usuarios",
            "client-key");

        Assert.True(result.NextCalled);
        Assert.Equal(true, result.Context.Items[ApiKeyDefaults.IsAdminItemKey]);
    }

    private static async Task<(bool NextCalled, DefaultHttpContext Context)> InvokeAsync(
        SecurityOptions security,
        string method,
        string path,
        string? apiKey)
    {
        var nextCalled = false;

        var middleware = new ApiKeyMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            Options.Create(security),
            NullLogger<ApiKeyMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        if (apiKey is not null)
        {
            context.Request.Headers[ApiKeyDefaults.HeaderName] = apiKey;
        }

        await middleware.InvokeAsync(context);

        return (nextCalled, context);
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
