using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

public sealed class UsuarioCambioPasswordDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario no es valido")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "La contrasena actual es obligatoria")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contrasena es obligatoria")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "La nueva contrasena debe tener entre 8 y 72 caracteres")]
    public string PasswordNueva { get; set; } = string.Empty;
}
