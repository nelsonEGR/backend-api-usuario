using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

/// <summary>Datos necesarios para crear un usuario.</summary>
public sealed class UsuarioCreateDto
{
    /// <summary>Nombre completo del usuario (2 a 150 caracteres).</summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Email del usuario. Debe ser unico y tener formato valido.</summary>
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    [StringLength(254, ErrorMessage = "El email no puede superar 254 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Numero de celular (opcional, maximo 20 caracteres).</summary>
    [StringLength(20, ErrorMessage = "El celular no puede superar 20 caracteres")]
    public string? Celular { get; set; }

    /// <summary>Fecha de nacimiento. Debe ser anterior a la fecha actual.</summary>
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    public DateTime FechaNacimiento { get; set; }

    /// <summary>Genero del usuario: 'M' o 'F'.</summary>
    [RegularExpression("^[MF]$", ErrorMessage = "El genero debe ser 'M' o 'F'")]
    public char Genero { get; set; }
}
