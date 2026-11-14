using ApiUsuario.Configuration;
using ApiUsuario.Extensions;
using ApiUsuario.Middleware;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Render inyecta PORT; en otros entornos se respeta ASPNETCORE_URLS.
builder.WebHost.ConfigureRenderPort();

builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

var securityOptions = app.Services.GetRequiredService<IOptions<SecurityOptions>>().Value;

if (securityOptions.IsApiKeyRequired()
    && string.IsNullOrWhiteSpace(securityOptions.ApiKey))
{
    throw new InvalidOperationException(
        "Falta la API key del servidor. Configure la variable de entorno " +
        "'Security__ApiKey' (o 'API_KEY') antes de iniciar la aplicacion.");
}

if (!app.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(securityOptions.AdminApiKey))
{
    app.Logger.LogWarning(
        "Security:AdminApiKey no esta configurada. La API key principal tendra permisos " +
        "administrativos. Configure 'Security__AdminApiKey' para separar ambos roles.");
}

// Render termina TLS y envia X-Forwarded-For / X-Forwarded-Proto.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseExceptionHandler();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (securityOptions.UseHstsEnabled(app.Environment))
{
    app.UseHsts();
}

var swaggerOptions = app.Services.GetRequiredService<IOptions<SwaggerSettings>>().Value;
if (swaggerOptions.IsEnabled(app.Environment))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    });
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok("API corriendo")).AllowAnonymous();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            })
        });
    }
}).AllowAnonymous();

app.MapControllers();

app.Run();

public partial class Program { }
