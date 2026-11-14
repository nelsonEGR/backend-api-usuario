using System.Globalization;
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
    /// <summary>Lista los usuarios registrados.</summary>
    /// <remarks>
    /// La paginacion es opcional: si se omiten 'page' y 'pageSize' se devuelven
    /// todos los usuarios (comportamiento historico). Si se envian, la respuesta
    /// incluye el header 'X-Total-Count' con el total de registros disponibles.
    /// </remarks>
    /// <param name="page">
    /// Numero de pagina, comenzando en 1. Omitir para no paginar.
    /// Si solo se envia pageSize, page toma el valor 1. Valores menores a 1 se
    /// ajustan a 1.
    /// </param>
    /// <param name="pageSize">
    /// Cantidad de registros por pagina. Omitir para no paginar.
    /// Si se envia, el valor se ajusta entre 1 y 200 (por defecto 50).
    /// </param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetUsuarios(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
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
}
