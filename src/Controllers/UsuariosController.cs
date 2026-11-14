using System.Globalization;
using ApiUsuario.Common.Exceptions;
using ApiUsuario.Dtos;
using ApiUsuario.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    /// <summary>Lista todos los usuarios o consulta uno por '?id'.</summary>
    /// <remarks>
    /// Con '?id=N' devuelve un usuario (404 si no existe). Sin 'id' devuelve todos.
    /// Paginacion opcional solo sin 'id': '?page=1&amp;pageSize=50'. En ese caso
    /// la respuesta incluye el header 'X-Total-Count'.
    /// </remarks>
    /// <param name="id">ID del usuario. Omitir para listar todos.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsuarios(
        [FromQuery] int? id,
        CancellationToken cancellationToken)
    {
        if (id.HasValue)
        {
            var usuario = await usuarioService.GetByIdAsync(id.Value, cancellationToken);
            return Ok(usuario);
        }

        var page = ReadOptionalInt("page");
        var pageSize = ReadOptionalInt("pageSize");

        var result = await usuarioService.GetAllAsync(page, pageSize, cancellationToken);

        Response.Headers["X-Total-Count"] = result.Total.ToString(CultureInfo.InvariantCulture);

        return Ok(result.Items);
    }

    /// <summary>Obtiene un usuario por su identificador.</summary>
    /// <param name="id">Identificador del usuario (mayor o igual a 1).</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(
        int id,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.GetByIdAsync(id, cancellationToken);
        return Ok(usuario);
    }

    /// <summary>Crea un nuevo usuario.</summary>
    /// <param name="dto">Datos del usuario a crear.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> PostUsuario(
        [FromBody] UsuarioCreateDto dto,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
    }

    /// <summary>Actualiza los datos de un usuario existente.</summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="dto">Datos a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutUsuario(
        int id,
        [FromBody] UsuarioUpdateDto dto,
        CancellationToken cancellationToken)
    {
        await usuarioService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>Elimina un usuario y su detalle asociado.</summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteUsuario(
        int id,
        CancellationToken cancellationToken)
    {
        await usuarioService.DeleteAsync(id, cancellationToken);

        return Ok(new { mensaje = $"El usuario con ID {id} fue eliminado correctamente." });
    }

    /// <summary>Activa la cuenta de un usuario inactivo.</summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpPatch("{id:int}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActivarUsuario(
        int id,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.ActivateAsync(id, cancellationToken);

        return Ok(new { mensaje = $"La cuenta de {usuario.Nombre} se activo correctamente." });
    }

    /// <summary>Desactiva la cuenta de un usuario activo.</summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpPatch("{id:int}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DesactivarUsuario(
        int id,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.DeactivateAsync(id, cancellationToken);

        return Ok(new { mensaje = $"La cuenta de {usuario.Nombre} se desactivo correctamente." });
    }

    /// <summary>
    /// Lee un parametro opcional de la query string sin exponerlo en Swagger.
    /// Devuelve null si no se envio y 400 si no es un entero valido.
    /// </summary>
    private int? ReadOptionalInt(string name)
    {
        var raw = Request.Query[name].ToString();

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new BadRequestException($"El parametro '{name}' debe ser un numero entero.");
        }

        return value;
    }
}
