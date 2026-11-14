using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Models;

public sealed class Usuario
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    public string Email { get; set; } = string.Empty;

    public string? Celular { get; set; }

    public DateTime FechaNacimiento { get; set; }

    public char Genero { get; set; }

    public short Estado { get; set; }

    public DateTime FechaAlta { get; set; }
}
