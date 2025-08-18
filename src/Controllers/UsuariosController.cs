using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_usuario.Data;
using api_usuario.Models;
using api_usuario.Dtos; // 👈 Importar los DTOs

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
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Celular = dto.Celular,
                Direccion = dto.Direccion,
                Ciudad = dto.Ciudad,
                Pais = dto.Pais,
                FechaNacimiento = dto.FechaNacimiento,
                Genero = dto.Genero,
                Estado = dto.Estado,
                FechaAlta = DateTime.UtcNow,
                FechaUltimoAcceso = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] UsuarioUpdateDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;
            usuario.Celular = dto.Celular;
            usuario.Direccion = dto.Direccion;
            usuario.Ciudad = dto.Ciudad;
            usuario.Pais = dto.Pais;
            usuario.FechaNacimiento = dto.FechaNacimiento;
            usuario.Genero = dto.Genero;
            usuario.Estado = dto.Estado;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}