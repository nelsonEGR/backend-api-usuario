namespace ApiUsuario.Models;

public sealed class UsuarioDetalle
{
    public int IdUsuario { get; set; }

    public int? IdDepartamento { get; set; }

    public int? IdCiudad { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime FechaUltimaActualizacion { get; set; }
}
