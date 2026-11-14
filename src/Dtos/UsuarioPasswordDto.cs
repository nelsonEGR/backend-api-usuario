using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

/// <summary>Datos para asignar la contrasena inicial de un usuario.</summary>
public sealed class UsuarioPasswordDto
{
    /// <summary>Identificador del usuario (mayor o igual a 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario no es valido")]
    public int IdUsuario { get; set; }

    /// <summary>Contrasena a asignar (8 a 72 caracteres).</summary>
    [Required(ErrorMessage = "La contrasena es obligatoria")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "La contrasena debe tener entre 8 y 72 caracteres")]
    public string Password { get; set; } = string.Empty;
}
