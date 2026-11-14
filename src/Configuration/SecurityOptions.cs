namespace ApiUsuario.Configuration;

/// <summary>
/// Opciones de seguridad de la API. Se enlazan desde la seccion "Security".
/// </summary>
public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    /// <summary>API key principal que envian los clientes en el header X-API-KEY.</summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// API key administrativa opcional. Si no se configura, la ApiKey principal
    /// se trata como administrativa por compatibilidad.
    /// </summary>
    public string? AdminApiKey { get; set; }

    /// <summary>
    /// Si es null, la API key es obligatoria. En Development se desactiva
    /// explicitamente desde appsettings.Development.json.
    /// </summary>
    public bool? RequireApiKey { get; set; }

    /// <summary>Activa la cabecera HSTS. Por defecto se activa en produccion.</summary>
    public bool? UseHsts { get; set; }

    public bool IsApiKeyRequired() => RequireApiKey ?? true;

    public bool UseHstsEnabled(IHostEnvironment environment) =>
        UseHsts ?? environment.IsProduction();
}
