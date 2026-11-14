using ApiUsuario.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiUsuario.HealthChecks;

/// <summary>Verifica que la base de datos PostgreSQL responda.</summary>
public sealed class DatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var canConnect = await db.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("Base de datos disponible.")
                : HealthCheckResult.Unhealthy("No fue posible conectar con la base de datos.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "No fue posible conectar con la base de datos.",
                exception);
        }
    }
}
