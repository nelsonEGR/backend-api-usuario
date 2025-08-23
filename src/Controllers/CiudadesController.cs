using api.Dtos;
using api_usuario.Data;
using api_usuario.Dtos;
using api_usuario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_usuario.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CiudadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CiudadesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCiudades(
            [FromQuery(Name = "id_ciudad")] int? idCiudad,
            [FromQuery(Name = "_id_departamento")] int? idDepartamento)
        {
            var query = _context.Ciudades.AsQueryable();

            if (idCiudad.HasValue)
                query = query.Where(c => c.IdCiudad == idCiudad.Value);

            if (idDepartamento.HasValue)
                query = query.Where(c => c.IdDepartamento == idDepartamento.Value);

            var ciudades = await query
                .Select(c => new CiudadDto
                {
                    IdCiudad = c.IdCiudad,
                    NombreCiudad = c.NombreCiudad,
                    IdDepartamento = c.IdDepartamento
                })
                .ToListAsync();

            if (ciudades.Count == 0)
            {
                return BadRequest(new
                {
                    codigo = 400,
                    mensaje = "No se encontraron ciudades con los filtros especificados"
                });
            }

            return Ok(ciudades);
        }
    }
}