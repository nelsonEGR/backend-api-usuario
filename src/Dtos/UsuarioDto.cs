namespace ApiUsuario.Dtos;

/// <summary>Representacion publica de un usuario (sin datos sensibles).</summary>
public sealed class UsuarioDto
{
    /// <summary>Identificador del usuario.</summary>
    public int IdUsuario { get; set; }

    /// <summary>Nombre completo.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Email del usuario.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Numero de celular (puede ser nulo).</summary>
    public string? Celular { get; set; }

    /// <summary>Fecha de nacimiento.</summary>
    public DateTime FechaNacimiento { get; set; }

    /// <summary>Genero: 'M' o 'F'.</summary>
    public char Genero { get; set; }

    /// <summary>Estado de la cuenta: 1 activo, 0 inactivo.</summary>
    public short Estado { get; set; }

    /// <summary>Fecha y hora de alta del usuario.</summary>
    public DateTime FechaAlta { get; set; }
}
