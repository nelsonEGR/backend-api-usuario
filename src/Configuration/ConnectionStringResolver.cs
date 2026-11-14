using Npgsql;

namespace ApiUsuario.Configuration;

/// <summary>
/// Resuelve la cadena de conexion desde configuracion o variables de entorno.
/// Nunca debe existir una cadena de conexion con credenciales en el repositorio.
/// </summary>
public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration, bool required = true)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (!required)
            {
                return string.Empty;
            }

            throw new InvalidOperationException(
                "No se configuro la cadena de conexion. Defina la variable de entorno " +
                "'ConnectionStrings__DefaultConnection' (o 'DATABASE_URL') en el proveedor de hosting.");
        }

        return Normalize(connectionString.Trim());
    }

    private static string Normalize(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            SslMode = SslMode.Require
        };

        foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = parameter.Split('=', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            var key = Uri.UnescapeDataString(parts[0]);
            var value = Uri.UnescapeDataString(parts[1]);

            switch (key.ToLowerInvariant())
            {
                case "sslmode":
                    builder.SslMode = value.ToLowerInvariant() switch
                    {
                        "disable" => SslMode.Disable,
                        "allow" => SslMode.Allow,
                        "prefer" => SslMode.Prefer,
                        "require" => SslMode.Require,
                        "verify-ca" => SslMode.VerifyCA,
                        "verify-full" => SslMode.VerifyFull,
                        _ => builder.SslMode
                    };
                    break;
                case "channel_binding" when value.Equals("require", StringComparison.OrdinalIgnoreCase):
                    builder.ChannelBinding = ChannelBinding.Require;
                    break;
            }
        }

        return builder.ConnectionString;
    }
}
