using System.Linq.Expressions;
using ApiUsuario.Dtos;
using ApiUsuario.Models;

namespace ApiUsuario.Mappings;

public static class UsuarioMappings
{
    public static readonly Expression<Func<Usuario, UsuarioDto>> Projection = usuario => new UsuarioDto
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

    private static readonly Func<Usuario, UsuarioDto> Map = Projection.Compile();

    public static UsuarioDto ToDto(this Usuario usuario) => Map(usuario);
}
