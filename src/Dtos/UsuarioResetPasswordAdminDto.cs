using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

public sealed class UsuarioResetPasswordAdminDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario no es valido")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "La nueva contrasena es obligatoria")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "La nueva contrasena debe tener entre 8 y 72 caracteres")]
    public string PasswordNueva { get; set; } = string.Empty;
}
