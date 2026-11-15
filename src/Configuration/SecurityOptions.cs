namespace ApiUsuario.Configuration;

/// <summary>
/// Opciones de seguridad de la API. Se enlazan desde la seccion "Security".
/// </summary>
public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    /// <summary>
    /// API key principal. Se envia en la cabecera 'X-API-KEY' en todas las
    /// peticiones (incluido POST /api/auth/token). Si no se configura, se usa
    /// la variable de entorno 'API_KEY'.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// API key administrativa opcional. Se envia en la cabecera 'X-API-KEY' y
    /// emite tokens con rol admin. Si no se configura, la ApiKey principal se
    /// trata como administrativa por compatibilidad.
    /// </summary>
    public string? AdminApiKey { get; set; }

    /// <summary>
    /// En Development, false desactiva la seguridad (access token y API key).
    /// En otros entornos siempre se exige, sin importar este valor.
    /// </summary>
    public bool? RequireApiKey { get; set; }

    /// <summary>Activa la cabecera HSTS. Por defecto se activa en produccion.</summary>
    public bool? UseHsts { get; set; }

    public bool IsApiKeyRequired() => RequireApiKey ?? true;

    public bool UseHstsEnabled(IHostEnvironment environment) =>
        UseHsts ?? environment.IsProduction();
}
