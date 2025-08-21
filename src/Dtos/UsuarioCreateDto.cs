namespace api_usuario.Dtos
{
    public class UsuarioCreateDto
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public string? Celular { get; set; } // Opcional
        public required DateTime FechaNacimiento { get; set; }
        public required char Genero { get; set; } // Validar 'M' o 'F' en el controlador o con atributos
    }
}