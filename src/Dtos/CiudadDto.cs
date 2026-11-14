namespace ApiUsuario.Dtos;

/// <summary>Ciudad dentro de un departamento.</summary>
public sealed class CiudadDto
{
    /// <summary>Identificador de la ciudad.</summary>
    public int IdCiudad { get; set; }

    /// <summary>Nombre de la ciudad.</summary>
    public string NombreCiudad { get; set; } = string.Empty;

    /// <summary>Identificador del departamento al que pertenece.</summary>
    public int IdDepartamento { get; set; }
}
