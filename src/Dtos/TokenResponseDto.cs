namespace ApiUsuario.Dtos;

/// <summary>Respuesta del login generico con el access token emitido.</summary>
public sealed class TokenResponseDto
{
    /// <summary>Token JWT que debe enviarse como "Authorization: Bearer".</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Tipo de token. Siempre "Bearer".</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>Segundos restantes antes de que el token expire.</summary>
    public int ExpiresIn { get; set; }

    /// <summary>Fecha y hora UTC en que expira el token.</summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
