using ApiUsuario.Data;
using ApiUsuario.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiUsuario.Services;

public sealed class DepartamentoService(AppDbContext db) : IDepartamentoService
{
    public async Task<IReadOnlyList<DepartamentoDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Departamentos
            .AsNoTracking()
            .OrderBy(d => d.IdDepartamento)
            .Select(d => new DepartamentoDto
            {
                IdDepartamento = d.IdDepartamento,
                NombreDepartamento = d.NombreDepartamento
            })
            .ToListAsync(cancellationToken);

    public async Task<DepartamentoDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await db.Departamentos
            .AsNoTracking()
            .Where(d => d.IdDepartamento == id)
            .Select(d => new DepartamentoDto
            {
                IdDepartamento = d.IdDepartamento,
                NombreDepartamento = d.NombreDepartamento
            })
            .FirstOrDefaultAsync(cancellationToken);
}
