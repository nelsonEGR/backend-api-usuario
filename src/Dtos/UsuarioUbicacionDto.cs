using System.ComponentModel.DataAnnotations;

public class UsuarioUbicacionDto
{
    [Required]
    public int IdCiudad { get; set; }

    [Required]
    public int IdDepartamento { get; set; }
}
