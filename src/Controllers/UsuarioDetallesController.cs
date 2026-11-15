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
    /// <param name="dto">Identificador del usuario y contrasena a asignar.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpPost("asignar-password")]
    [Authorize(Policy = Policies.Admin)]
    [EnableRateLimiting("sensitive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AsignarPassword(
        [FromBody] UsuarioPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await usuarioDetalleService.AssignPasswordAsync(dto, cancellationToken);

        return Ok(new { mensaje = "Contrasena asignada correctamente" });
    }

    /// <summary>Cambia la contrasena verificando la contrasena actual.</summary>
    /// <param name="dto">Identificador, contrasena actual y nueva contrasena.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
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
    /// <param name="dto">Identificador del usuario y nueva contrasena.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpPatch("reset-password")]
    [Authorize(Policy = Policies.Admin)]
    [EnableRateLimiting("sensitive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] UsuarioResetPasswordAdminDto dto,
        CancellationToken cancellationToken)
    {
        await usuarioDetalleService.ResetPasswordAsync(dto, cancellationToken);

        return Ok(new { mensaje = "Contrasena restablecida correctamente" });
    }

    /// <summary>Actualiza el departamento y la ciudad de un usuario.</summary>
    /// <param name="idUsuario">Identificador del usuario.</param>
    /// <param name="dto">Departamento y ciudad destino.</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpPut("{idUsuario}/ubicacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarUbicacionUsuario(
        int idUsuario,
        [FromBody] UsuarioUbicacionDto dto,
        CancellationToken cancellationToken)
    {
        await usuarioDetalleService.UpdateLocationAsync(idUsuario, dto, cancellationToken);

        return Ok(new { mensaje = "Ubicacion actualizada correctamente." });
    }
}
