namespace ApiUsuario.Dtos;

/// <summary>Departamento geografico.</summary>
public sealed class DepartamentoDto
{
    /// <summary>Identificador del departamento.</summary>
    public int IdDepartamento { get; set; }

    /// <summary>Nombre del departamento.</summary>
    public string NombreDepartamento { get; set; } = string.Empty;
}
