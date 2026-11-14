using ApiUsuario.Dtos;
using ApiUsuario.Models;

namespace ApiUsuario.Mappings;

public static class UsuarioMappings
{
    public static UsuarioDto ToDto(this Usuario usuario) => new()
    {
        IdUsuario = usuario.IdUsuario,
        Nombre = usuario.Nombre,
        Email = usuario.Email,
        Celular = usuario.Celular,
        FechaNacimiento = usuario.FechaNacimiento,
        Genero = usuario.Genero,
        Estado = usuario.Estado,
        FechaAlta = usuario.FechaAlta
    };
}
