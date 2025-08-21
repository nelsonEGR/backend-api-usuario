namespace api_usuario.Dtos
{
    public class UsuarioUpdateDto
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public string? Celular { get; set; }
        public required DateTime FechaNacimiento { get; set; }
        public required char Genero { get; set; }
    }
}