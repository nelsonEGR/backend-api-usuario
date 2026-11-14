namespace ApiUsuario.Models;

public sealed class Ciudad
{
    public int IdCiudad { get; set; }

    public string NombreCiudad { get; set; } = string.Empty;

    public int IdDepartamento { get; set; }

    public Departamento? Departamento { get; set; }
}
