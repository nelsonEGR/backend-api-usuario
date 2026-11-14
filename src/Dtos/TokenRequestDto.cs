namespace ApiUsuario.Dtos;

/// <summary>
/// Credenciales del login generico. El secreto es la API key configurada
/// en el servidor (Security:ApiKey o Security:AdminApiKey).
/// </summary>
public sealed class TokenRequestDto
{
    /// <summary>
    /// Identificador del cliente. Es opcional; si se omite se usa "generic-client".
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>API key del cliente (Security:ApiKey o Security:AdminApiKey).</summary>
    public string? ClientSecret { get; set; }
}
