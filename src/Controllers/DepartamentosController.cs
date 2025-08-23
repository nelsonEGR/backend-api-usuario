using api_usuario.Data;
using api_usuario.Dtos;      // 👈 Cambié para que apunte al namespace correcto
using api_usuario.Models;    // 👈 Cambié para que apunte al namespace correcto
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_usuario.Controllers // 👈 Cambié para mantener coherencia con el proyecto
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartamentosController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/departamentos
        // GET /api/departamentos?id=1
        [HttpGet]
        public async Task<IActionResult> GetDepartamentos([FromQuery] int? id)
        {
            if (id.HasValue)
            {
                var departamento = await _context.Departamentos
                    .Where(d => d.IdDepartamento == id.Value)
                    .Select(d => new DepartamentoDto
                    {
                        IdDepartamento = d.IdDepartamento,
                        NombreDepartamento = d.NombreDepartamento
                    })
                    .FirstOrDefaultAsync();

                if (departamento == null)
                {
                    return BadRequest(new
                    {
                        codigo = 400,
                        mensaje = $"No existe el departamento con ID {id.Value}"
                    });
                }

                return Ok(departamento);
            }
            else
            {
                var departamentos = await _context.Departamentos
                    .Select(d => new DepartamentoDto
                    {
                        IdDepartamento = d.IdDepartamento,
                        NombreDepartamento = d.NombreDepartamento
                    })
                    .ToListAsync();

                return Ok(departamentos);
            }
        }
    }
}