using Microsoft.AspNetCore.Hosting;

namespace ApiUsuario.Extensions;

public static class WebHostExtensions
{
    /// <summary>
    /// Render inyecta la variable PORT. Kestrel debe escuchar en esa interfaz.
    /// </summary>
    public static IWebHostBuilder ConfigureRenderPort(this IWebHostBuilder webHost)
    {
        var portValue = Environment.GetEnvironmentVariable("PORT");

        if (int.TryParse(portValue, out var port) && port > 0)
        {
            webHost.ConfigureKestrel(options => options.ListenAnyIP(port));
        }

        return webHost;
    }
}
