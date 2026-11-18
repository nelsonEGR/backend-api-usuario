namespace ApiUsuario.Configuration;

/// <summary>
/// Limites de peticiones por IP. Se enlazan desde la seccion "RateLimiting".
/// </summary>
public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    /// <summary>Activa o desactiva por completo el limite de peticiones.</summary>
    public bool Enabled { get; set; }

    public int PermitLimit { get; set; } = 120;

    public int WindowSeconds { get; set; } = 60;

    /// <summary>Limite mas estricto para endpoints sensibles (passwords).</summary>
    public int SensitivePermitLimit { get; set; } = 10;

    public int SensitiveWindowSeconds { get; set; } = 60;
}
