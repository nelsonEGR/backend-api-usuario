using ApiUsuario.Configuration;
using ApiUsuario.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApiUsuario.Tests.Middleware;

public sealed class ApiKeyValidationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_EndpointProtegidoSinApiKey_Devuelve401()
    {
        var called = false;
        var context = CreateContext(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true },
            allowAnonymous: false);
        var middleware = new ApiKeyValidationMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.False(called);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_EndpointProtegidoConApiKeyValida_LlamaAlSiguiente()
    {
        var called = false;
        var context = CreateContext(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true },
            allowAnonymous: false);
        context.Request.Headers["X-API-KEY"] = "client-key";
        var middleware = new ApiKeyValidationMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_EndpointProtegidoConAdminApiKey_LlamaAlSiguiente()
    {
        var called = false;
        var context = CreateContext(
            new SecurityOptions
            {
                ApiKey = "client-key",
                AdminApiKey = "admin-key",
                RequireApiKey = true
            },
            allowAnonymous: false);
        context.Request.Headers["X-API-KEY"] = "admin-key";
        var middleware = new ApiKeyValidationMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_EndpointAnonimoSinApiKey_LlamaAlSiguiente()
    {
        var called = false;
        var context = CreateContext(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true },
            allowAnonymous: true);
        var middleware = new ApiKeyValidationMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_SinApiKeyRequerida_LlamaAlSiguiente()
    {
        var called = false;
        var context = CreateContext(
            new SecurityOptions { RequireApiKey = false },
            allowAnonymous: false);
        var middleware = new ApiKeyValidationMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    private static DefaultHttpContext CreateContext(SecurityOptions security, bool allowAnonymous)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptionsMonitor<SecurityOptions>>(
            new TestOptionsMonitor<SecurityOptions>(security));

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Response.Body = new MemoryStream();

        var metadata = allowAnonymous
            ? new EndpointMetadataCollection(new AllowAnonymousAttribute())
            : new EndpointMetadataCollection();

        context.SetEndpoint(new Endpoint(requestDelegate: null, metadata, "test"));

        return context;
    }

    private sealed class TestOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue { get; } = value;

        public T Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
