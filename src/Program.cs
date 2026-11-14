using System.Globalization;
using ApiUsuario.Configuration;
using ApiUsuario.Extensions;
using ApiUsuario.Logging;
using ApiUsuario.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using Serilog.Debugging;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration.GetValue<bool>("Serilog:SelfLog"))
{
    SelfLog.Enable(Console.Error.WriteLine);
}

// Render inyecta PORT; en otros entornos se respeta ASPNETCORE_URLS.
builder.WebHost.ConfigureRenderPort();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "backend-api-usuario")
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);

    var lokiUrl = context.Configuration["Loki:Url"];

    if (!string.IsNullOrWhiteSpace(lokiUrl))
    {
        configuration.WriteTo.GrafanaLoki(
            lokiUrl,
            labels: [new LokiLabel { Key = "app", Value = "backend-api-usuario" }],
            credentials: new LokiCredentials
            {
                Login = context.Configuration["Loki:User"] ?? string.Empty,
                Password = context.Configuration["Loki:ApiKey"] ?? string.Empty
            });
    }
});

builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.Logger.LogInformation(
    "Logging configurado. Loki: {Estado}",
    string.IsNullOrWhiteSpace(builder.Configuration["Loki:Url"]) ? "deshabilitado" : "habilitado");

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
    StartupLog.AdminApiKeyMissing(app.Logger);
}

app.Logger.LogInformation(
    "Seguridad: {Estado}",
    securityOptions.IsApiKeyRequired()
        ? "access token obligatorio en rutas protegidas; API key requerida como clientSecret en /api/auth/token"
        : "desactivada en Development (sin token ni API key)");

// Render termina TLS y envia X-Forwarded-For / X-Forwarded-Proto.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

app.Use(async (context, next) =>
{
    var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var header)
        && !string.IsNullOrWhiteSpace(header)
            ? header.ToString()
            : Guid.NewGuid().ToString("N");

    context.TraceIdentifier = requestId;
    context.Response.Headers["X-Request-Id"] = requestId;

    using (LogContext.PushProperty("RequestId", requestId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();
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

        // Oculta la seccion "Schemas" del final. La API se usa para obtener
        // las URLs/curl, no para revisar los modelos.
        options.DefaultModelsExpandDepth(-1);
    });
}

app.UseRouting();
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

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
