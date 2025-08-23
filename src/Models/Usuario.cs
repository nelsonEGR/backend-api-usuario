using System.ComponentModel.DataAnnotations;

namespace api_usuario.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        public string? Celular { get; set; } // Opcional según SQL

        public DateTime FechaNacimiento { get; set; }

        public char Genero { get; set; } // CHAR(1) en SQL

        public short Estado { get; set; } // SMALLINT en SQL

        public DateTime FechaAlta { get; set; } // TIMESTAMP en SQL
    }
}