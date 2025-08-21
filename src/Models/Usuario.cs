namespace api_usuario.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string? Celular { get; set; } // Opcional según SQL
        public DateTime FechaNacimiento { get; set; }
        public char Genero { get; set; } // CHAR(1) en SQL
        public short Estado { get; set; } // SMALLINT en SQL
        public DateTime FechaAlta { get; set; } // TIMESTAMP en SQL
    }
}