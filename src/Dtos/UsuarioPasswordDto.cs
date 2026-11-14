using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

public sealed class UsuarioPasswordDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario no es valido")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "La contrasena es obligatoria")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "La contrasena debe tener entre 8 y 72 caracteres")]
    public string Password { get; set; } = string.Empty;
}
