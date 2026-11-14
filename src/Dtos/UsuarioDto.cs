namespace ApiUsuario.Dtos;

/// <summary>Representacion publica de un usuario (sin datos sensibles).</summary>
public sealed class UsuarioDto
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public char Genero { get; set; }
    public short Estado { get; set; }
    public DateTime FechaAlta { get; set; }
}
