namespace ApiUsuario.Configuration;

/// <summary>
/// Configuracion de CORS. Se enlaza desde la seccion "Cors".
/// </summary>
public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    /// <summary>Origenes permitidos, por ejemplo https://mi-frontend.com.</summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Mantiene el comportamiento historico de permitir cualquier origen.
    /// En produccion se recomienda establecerlo en false y configurar AllowedOrigins.
    /// </summary>
    public bool AllowAnyOrigin { get; set; }
}
