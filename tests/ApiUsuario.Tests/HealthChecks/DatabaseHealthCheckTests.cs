using ApiUsuario.Data;
using ApiUsuario.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiUsuario.Tests.HealthChecks;

public sealed class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ConEsquemaDisponible_DevuelveHealthy()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"health-{Guid.NewGuid()}"));

        await using var provider = services.BuildServiceProvider();
        var check = new DatabaseHealthCheck(provider.GetRequiredService<IServiceScopeFactory>());

        var result = await check.CheckHealthAsync(
            new HealthCheckContext(),
            CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }
}
