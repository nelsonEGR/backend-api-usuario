using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

public sealed class UsuarioUbicacionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El ID de ciudad no es valido")]
    public int IdCiudad { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El ID de departamento no es valido")]
    public int IdDepartamento { get; set; }
}
