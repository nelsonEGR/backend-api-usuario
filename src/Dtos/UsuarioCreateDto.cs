using System;

namespace api_usuario.Dtos
{
    public class UsuarioCreateDto
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Celular { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; }
        public string Estado { get; set; }
    }
}