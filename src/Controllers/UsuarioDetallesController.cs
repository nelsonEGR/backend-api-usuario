using api_usuario.Data;
using api_usuario.Dtos;
using api_usuario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_usuario.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioDetallesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioDetallesController(AppDbContext context)
        {
            _context = context;
        }

        // 1️⃣ Asignar contraseña inicial
        [HttpPost("asignar-password")]
        public async Task<IActionResult> AsignarPassword([FromBody] UsuarioPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { mensaje = "La contraseña no puede estar vacía" });

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == dto.IdUsuario);
            if (!usuarioExiste)
                return BadRequest(new { mensaje = "El usuario no existe" });

            var detalleExiste = await _context.UsuarioDetalles.FindAsync(dto.IdUsuario);
            if (detalleExiste != null)
                return BadRequest(new { mensaje = "El usuario ya tiene contraseña asignada" });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var nuevoDetalle = new UsuarioDetalle
            {
                IdUsuario = dto.IdUsuario,
                IdDepartamento = null,
                IdCiudad = null,
                PasswordHash = passwordHash,
                FechaUltimaActualizacion = DateTime.UtcNow
            };

            _context.UsuarioDetalles.Add(nuevoDetalle);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña asignada correctamente" });
        }

        // 2️⃣ Cambiar contraseña (requiere contraseña actual)
        [HttpPatch("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] UsuarioCambioPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PasswordActual) || string.IsNullOrWhiteSpace(dto.PasswordNueva))
                return BadRequest(new { mensaje = "Debe ingresar la contraseña actual y la nueva" });

            var detalle = await _context.UsuarioDetalles
                .FirstOrDefaultAsync(d => d.IdUsuario == dto.IdUsuario);

            if (detalle == null)
                return BadRequest(new { mensaje = "El usuario no tiene contraseña asignada" });

            bool esValida = BCrypt.Net.BCrypt.Verify(dto.PasswordActual, detalle.PasswordHash);
            if (!esValida)
                return BadRequest(new { mensaje = "La contraseña actual es incorrecta" });

            detalle.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
            detalle.FechaUltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña actualizada correctamente" });
        }

        // 3️⃣ Resetear contraseña (sin contraseña actual)
        [HttpPatch("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UsuarioResetPasswordAdminDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PasswordNueva))
                return BadRequest(new { mensaje = "La nueva contraseña no puede estar vacía" });

            var detalle = await _context.UsuarioDetalles
                .FirstOrDefaultAsync(d => d.IdUsuario == dto.IdUsuario);

            if (detalle == null)
                return BadRequest(new { mensaje = "El usuario no tiene detalle registrado" });

            detalle.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
            detalle.FechaUltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña restablecida correctamente" });
        }

        [HttpPut("{idUsuario}/ubicacion")]
        public async Task<IActionResult> ActualizarUbicacionUsuario(int idUsuario, [FromBody] UsuarioUbicacionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1️⃣ Verificar que exista el detalle del usuario
            var usuarioDetalle = await _context.UsuarioDetalles
                .FirstOrDefaultAsync(ud => ud.IdUsuario == idUsuario);

            if (usuarioDetalle == null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe un registro de detalle para el usuario con ID {idUsuario}."
                });
            }

            // 2️⃣ Validar existencia de ciudad
            bool ciudadExiste = await _context.Ciudades
                .AnyAsync(c => c.IdCiudad == dto.IdCiudad);

            if (!ciudadExiste)
            {
                return NotFound(new
                {
                    mensaje = $"La ciudad con ID {dto.IdCiudad} no existe."
                });
            }

            // 3️⃣ Validar existencia de departamento
            bool departamentoExiste = await _context.Departamentos
                .AnyAsync(d => d.IdDepartamento == dto.IdDepartamento);

            if (!departamentoExiste)
            {
                return NotFound(new
                {
                    mensaje = $"El departamento con ID {dto.IdDepartamento} no existe."
                });
            }

            // 3.1️⃣ Validar que la ciudad pertenezca al departamento indicado
            bool ciudadPerteneceAlDepartamento = await _context.Ciudades
                .AnyAsync(c => c.IdCiudad == dto.IdCiudad && c.IdDepartamento == dto.IdDepartamento);

            if (!ciudadPerteneceAlDepartamento)
            {
                return BadRequest(new
                {
                    mensaje = $"La ciudad con ID {dto.IdCiudad} no pertenece al departamento con ID {dto.IdDepartamento}."
                });
            }

            // 4️⃣ Actualizar solo los campos permitidos
            usuarioDetalle.IdCiudad = dto.IdCiudad;
            usuarioDetalle.IdDepartamento = dto.IdDepartamento;
            usuarioDetalle.FechaUltimaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Ubicación actualizada correctamente."
            });
        }













    }
}