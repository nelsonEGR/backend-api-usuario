// Models/Ciudad.cs
using api_usuario.Models;

namespace api.Models
{
    public class Ciudad
    {
        public int IdCiudad { get; set; }
        public string NombreCiudad { get; set; } = string.Empty;

        // FK
        public int IdDepartamento { get; set; }
        public Departamento? Departamento { get; set; }
    }
}