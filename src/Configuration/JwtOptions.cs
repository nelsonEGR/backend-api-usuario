namespace ApiUsuario.Configuration;

/// <summary>
/// Opciones de firma y validacion de los access tokens JWT.
/// Se enlazan desde la seccion "Jwt".
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Emisor del token (claim iss).</summary>
    public string Issuer { get; set; } = "api-usuario";

    /// <summary>Audiencia del token (claim aud).</summary>
    public string Audience { get; set; } = "api-usuario-clients";

    /// <summary>
    /// Clave simetrica usada para firmar. Debe tener al menos 32 caracteres.
    /// En produccion es obligatoria y se inyecta por variable de entorno.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Minutos de validez del access token. Se limita entre 5 y 1440.
    /// Recomendado: 5 minutos para un token generico de servicio.
    /// </summary>
    public int ExpiresMinutes { get; set; } = 5;
}
