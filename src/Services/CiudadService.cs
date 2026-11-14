using ApiUsuario.Data;
using ApiUsuario.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiUsuario.Services;

public sealed class CiudadService(AppDbContext db) : ICiudadService
{
    public async Task<IReadOnlyList<CiudadDto>> GetAsync(
        int? idCiudad,
        int? idDepartamento,
        CancellationToken cancellationToken)
    {
        var query = db.Ciudades.AsNoTracking();

        if (idCiudad.HasValue)
        {
            query = query.Where(c => c.IdCiudad == idCiudad.Value);
        }

        if (idDepartamento.HasValue)
        {
            query = query.Where(c => c.IdDepartamento == idDepartamento.Value);
        }

        return await query
            .OrderBy(c => c.IdCiudad)
            .Select(c => new CiudadDto
            {
                IdCiudad = c.IdCiudad,
                NombreCiudad = c.NombreCiudad,
                IdDepartamento = c.IdDepartamento
            })
            .ToListAsync(cancellationToken);
    }
}
