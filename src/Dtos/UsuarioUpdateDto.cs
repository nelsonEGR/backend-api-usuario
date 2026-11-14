using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

public sealed class UsuarioUpdateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    [StringLength(254, ErrorMessage = "El email no puede superar 254 caracteres")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El celular no puede superar 20 caracteres")]
    public string? Celular { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    public DateTime FechaNacimiento { get; set; }

    [RegularExpression("^[MF]$", ErrorMessage = "El genero debe ser 'M' o 'F'")]
    public char Genero { get; set; }
}
