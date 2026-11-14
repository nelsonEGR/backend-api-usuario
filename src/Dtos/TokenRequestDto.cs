namespace ApiUsuario.Dtos;

/// <summary>
/// Credenciales del login generico. El secreto es la API key configurada
/// en el servidor (Security:ApiKey o Security:AdminApiKey).
/// </summary>
public sealed class TokenRequestDto
{
    /// <summary>API key del cliente (Security:ApiKey o Security:AdminApiKey).</summary>
    public string? ClientSecret { get; set; }
}
