using System.Text;
using System.Text.Encodings.Web;
using ApiUsuario.Configuration;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ApiUsuario.Tests.Security;

public sealed class ApiKeyAuthenticationHandlerTests
{
    private const string HeaderName = "X-API-KEY";

    [Fact]
    public async Task Authenticate_ConApiKeyCliente_DevuelveRolClient()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[HeaderName] = "client-key";

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = "client-key", AdminApiKey = "admin-key", RequireApiKey = true },
            context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.True(result.Principal!.IsInRole("client"));
    }

    [Fact]
    public async Task Authenticate_ConAdminApiKey_DevuelveRolAdmin()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[HeaderName] = "admin-key";

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = "client-key", AdminApiKey = "admin-key", RequireApiKey = true },
            context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.True(result.Principal!.IsInRole("admin"));
    }

    [Fact]
    public async Task Authenticate_SinApiKeyAdministrativa_ApiKeyPrincipalEsAdmin()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[HeaderName] = "only-key";

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = "only-key", AdminApiKey = null, RequireApiKey = true },
            context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.True(result.Principal!.IsInRole("admin"));
    }

    [Fact]
    public async Task Authenticate_SinHeader_DevuelveNoResult()
    {
        var context = new DefaultHttpContext();

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true },
            context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.None);
    }

    [Fact]
    public async Task Authenticate_ConApiKeyInvalida_DevuelveFailure()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[HeaderName] = "clave-incorrecta";

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true },
            context);

        var result = await handler.AuthenticateAsync();

        Assert.NotNull(result.Failure);
    }

    [Fact]
    public async Task Authenticate_SinApiKeyConfiguradaEnServidor_DevuelveFailure()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[HeaderName] = "cualquiera";

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = null, RequireApiKey = true },
            context);

        var result = await handler.AuthenticateAsync();

        Assert.NotNull(result.Failure);
    }

    [Fact]
    public async Task Authenticate_SiNoSeRequiereApiKey_EsAdmin()
    {
        var context = new DefaultHttpContext();

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = null, RequireApiKey = false },
            context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.True(result.Principal!.IsInRole("admin"));
    }

    [Fact]
    public async Task Challenge_EscribeProblemJson401()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true },
            context);

        await handler.ChallengeAsync(properties: null);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Contains("application/problem+json", context.Response.ContentType);
        Assert.Contains("mensaje", await ReadBodyAsync(context.Response));
    }

    [Fact]
    public async Task Forbid_EscribeProblemJson403()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handler = await CreateHandlerAsync(
            new SecurityOptions { ApiKey = "client-key", RequireApiKey = true },
            context);

        await handler.ForbidAsync(properties: null);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Contains("mensaje", await ReadBodyAsync(context.Response));
    }

    private static async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(
        SecurityOptions security,
        DefaultHttpContext context)
    {
        var handler = new ApiKeyAuthenticationHandler(
            new TestOptionsMonitor<ApiKeyAuthenticationOptions>(new ApiKeyAuthenticationOptions()),
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            new TestOptionsMonitor<SecurityOptions>(security));

        var scheme = new AuthenticationScheme(
            ApiKeyAuthenticationOptions.DefaultScheme,
            displayName: null,
            handlerType: typeof(ApiKeyAuthenticationHandler));

        await handler.InitializeAsync(scheme, context);
        return handler;
    }

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

    private sealed class TestOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue { get; } = value;

        public T Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
