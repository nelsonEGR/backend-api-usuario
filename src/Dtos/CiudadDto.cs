namespace ApiUsuario.Dtos;

public sealed class CiudadDto
{
    public int IdCiudad { get; set; }
    public string NombreCiudad { get; set; } = string.Empty;
    public int IdDepartamento { get; set; }
}
