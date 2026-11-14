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
    /// <summary>Lista usuarios. Si se envian page/pageSize se aplica paginacion.</summary>
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
