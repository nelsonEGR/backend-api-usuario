using Microsoft.Extensions.Logging;

namespace ApiUsuario.Logging;

/// <summary>
/// Mensajes de arranque generados con LoggerMessage para evitar asignaciones
/// y mantener el build sin advertencias CA1848.
/// </summary>
internal static partial class StartupLog
{
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Warning,
        Message = "Security:AdminApiKey no esta configurada. La API key principal tendra permisos " +
                  "administrativos. Configure 'Security__AdminApiKey' para separar ambos roles.")]
    public static partial void AdminApiKeyMissing(ILogger logger);
}
