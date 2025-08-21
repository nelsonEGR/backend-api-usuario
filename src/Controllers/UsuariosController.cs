using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_usuario.Data;
using api_usuario.Models;
using api_usuario.Dtos;

namespace api_usuario.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios() =>
            await _context.Usuarios.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return usuario;
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario([FromBody] UsuarioCreateDto dto)
        {
            if (dto.Genero != 'M' && dto.Genero != 'F')
                return BadRequest("El género debe ser 'M' o 'F'");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Celular = dto.Celular,
                FechaNacimiento = dto.FechaNacimiento,
                Genero = dto.Genero,
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (dto.Genero != 'M' && dto.Genero != 'F')
                return BadRequest("El género debe ser 'M' o 'F'");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;
            usuario.Celular = dto.Celular;
            usuario.FechaNacimiento = dto.FechaNacimiento;
            usuario.Genero = dto.Genero;

            await _context.SaveChangesAsync();
            return NoContent();
        }

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUsuario(string id)
{
    if (string.IsNullOrWhiteSpace(id))
        return BadRequest(new { mensaje = "El ID no puede estar vacío. Debe proporcionar un valor numérico válido." });

    if (!int.TryParse(id, out int idInt) || idInt <= 0)
        return BadRequest(new { mensaje = "El ID proporcionado no es un número válido." });

    var usuario = await _context.Usuarios.FindAsync(idInt);
    if (usuario == null)
        return BadRequest(new { mensaje = $"El usuario con ID {idInt} no existe." });

    if (usuario.Estado == 1)
        return BadRequest(new { mensaje = "El usuario se encuentra activo y no puede ser eliminado." });

    _context.Usuarios.Remove(usuario);
    await _context.SaveChangesAsync();

    return Ok(new { mensaje = $"El usuario con ID {idInt} fue eliminado correctamente." });
}
        /// PATCH: Activa la cuenta del usuario si está inactiva
        [HttpPatch("{id}/activar")]
        public async Task<IActionResult> ActivarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = $"El usuario con ID {id} no existe." });

            if (usuario.Estado == 1)
                return BadRequest(new { mensaje = $"La cuenta de {usuario.Nombre} ya está activa." });

            usuario.Estado = 1;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"La cuenta de {usuario.Nombre} se activó correctamente." });
        }
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = $"El usuario con ID {id} no existe." });

            if (usuario.Estado == 0)
                return BadRequest(new { mensaje = $"La cuenta de {usuario.Nombre} ya está inactiva." });

            usuario.Estado = 0;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"La cuenta de {usuario.Nombre} se desactivó correctamente." });
        }

    }
}

