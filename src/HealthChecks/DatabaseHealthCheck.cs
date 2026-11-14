using ApiUsuario.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiUsuario.HealthChecks;

/// <summary>
/// Verifica que PostgreSQL responda y que existan las tablas y columnas que usa
/// la aplicacion. Asi se detecta una base vacia o apuntando a otra rama antes de
/// que los endpoints empiecen a devolver 500.
/// </summary>
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

            if (!await db.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Unhealthy("No fue posible conectar con la base de datos.");
            }

            // Take(1) valida tabla y columnas mapeadas sin traer datos masivos.
            await db.Usuarios.AsNoTracking().Take(1).ToListAsync(cancellationToken);
            await db.Departamentos.AsNoTracking().Take(1).ToListAsync(cancellationToken);
            await db.Ciudades.AsNoTracking().Take(1).ToListAsync(cancellationToken);
            await db.UsuarioDetalles.AsNoTracking().Take(1).ToListAsync(cancellationToken);

            return HealthCheckResult.Healthy("Base de datos y esquema disponibles.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "La base de datos no responde o no tiene el esquema esperado.",
                exception);
        }
    }
}
