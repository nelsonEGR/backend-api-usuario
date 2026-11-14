using ApiUsuario.Dtos;
using ApiUsuario.Security;
using ApiUsuario.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiUsuario.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class UsuarioDetallesController(IUsuarioDetalleService usuarioDetalleService)
    : ControllerBase
{
    /// <summary>Asigna la contrasena inicial. Requiere permisos administrativos.</summary>
    [HttpPost("asignar-password")]
    [Authorize(Policy = Policies.Admin)]
    [EnableRateLimiting("sensitive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarPassword(
        [FromBody] UsuarioPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await usuarioDetalleService.AssignPasswordAsync(dto, cancellationToken);

        return Ok(new { mensaje = "Contrasena asignada correctamente" });
    }

    /// <summary>Cambia la contrasena verificando la contrasena actual.</summary>
    [HttpPatch("cambiar-password")]
    [EnableRateLimiting("sensitive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarPassword(
        [FromBody] UsuarioCambioPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await usuarioDetalleService.ChangePasswordAsync(dto, cancellationToken);

        return Ok(new { mensaje = "Contrasena actualizada correctamente" });
    }

    /// <summary>Restablece la contrasena sin solicitar la actual. Requiere permisos administrativos.</summary>
    [HttpPatch("reset-password")]
    [Authorize(Policy = Policies.Admin)]
    [EnableRateLimiting("sensitive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] UsuarioResetPasswordAdminDto dto,
        CancellationToken cancellationToken)
    {
        await usuarioDetalleService.ResetPasswordAsync(dto, cancellationToken);

        return Ok(new { mensaje = "Contrasena restablecida correctamente" });
    }

    [HttpPut("{idUsuario:int}/ubicacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarUbicacionUsuario(
        int idUsuario,
        [FromBody] UsuarioUbicacionDto dto,
        CancellationToken cancellationToken)
    {
        await usuarioDetalleService.UpdateLocationAsync(idUsuario, dto, cancellationToken);

        return Ok(new { mensaje = "Ubicacion actualizada correctamente." });
    }
}
