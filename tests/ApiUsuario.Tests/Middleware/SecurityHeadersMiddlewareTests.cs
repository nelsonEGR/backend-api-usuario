using ApiUsuario.Middleware;
using Microsoft.AspNetCore.Http;

namespace ApiUsuario.Tests.Middleware;

public sealed class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AgregaCabecerasDeSeguridad()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"].ToString());
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"].ToString());
        Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"].ToString());
        Assert.Equal(
            "camera=(), microphone=(), geolocation=()",
            context.Response.Headers["Permissions-Policy"].ToString());
        Assert.Equal("none", context.Response.Headers["X-Permitted-Cross-Domain-Policies"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_LlamaAlSiguienteMiddleware()
    {
        var called = false;
        var middleware = new SecurityHeadersMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(new DefaultHttpContext());

        Assert.True(called);
    }
}
