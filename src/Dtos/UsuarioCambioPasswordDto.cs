using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

/// <summary>Datos para cambiar la contrasena verificando la actual.</summary>
public sealed class UsuarioCambioPasswordDto
{
    /// <summary>Identificador del usuario (mayor o igual a 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario no es valido")]
    public int IdUsuario { get; set; }

    /// <summary>Contrasena actual del usuario.</summary>
    [Required(ErrorMessage = "La contrasena actual es obligatoria")]
    public string PasswordActual { get; set; } = string.Empty;

    /// <summary>Nueva contrasena (8 a 72 caracteres).</summary>
    [Required(ErrorMessage = "La nueva contrasena es obligatoria")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "La nueva contrasena debe tener entre 8 y 72 caracteres")]
    public string PasswordNueva { get; set; } = string.Empty;
}
