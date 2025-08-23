namespace api_usuario.Models
{
    public class UsuarioDetalle
    {
        public int IdUsuario { get; set; }
        public int? IdDepartamento { get; set; } // Puede ser null
        public int? IdCiudad { get; set; }       // Puede ser null
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime FechaUltimaActualizacion { get; set; }
    }
}