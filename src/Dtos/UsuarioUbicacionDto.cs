using System.ComponentModel.DataAnnotations;

namespace ApiUsuario.Dtos;

/// <summary>Ubicacion (departamento y ciudad) de un usuario.</summary>
public sealed class UsuarioUbicacionDto
{
    /// <summary>Identificador de la ciudad (mayor o igual a 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID de ciudad no es valido")]
    public int IdCiudad { get; set; }

    /// <summary>Identificador del departamento (mayor o igual a 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID de departamento no es valido")]
    public int IdDepartamento { get; set; }
}
