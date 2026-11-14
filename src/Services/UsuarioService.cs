using ApiUsuario.Common.Exceptions;
using ApiUsuario.Common.Models;
using ApiUsuario.Data;
using ApiUsuario.Dtos;
using ApiUsuario.Mappings;
using ApiUsuario.Models;
using Microsoft.EntityFrameworkCore;

// ToLower se usa dentro de expresiones LINQ traducidas por EF Core; las reglas
// de cultura no aplican en SQL.
#pragma warning disable CA1304, CA1311, CA1862

namespace ApiUsuario.Services;

public sealed class UsuarioService(AppDbContext db) : IUsuarioService
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    public async Task<PagedResult<UsuarioDto>> GetAllAsync(
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Usuario> query = db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.IdUsuario);

        var isPaginated = page.HasValue || pageSize.HasValue;
        var total = 0;

        if (isPaginated)
        {
            total = await db.Usuarios.CountAsync(cancellationToken);

            var currentPage = Math.Max(1, page ?? 1);
            var size = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);

            query = query
                .Skip((currentPage - 1) * size)
                .Take(size);
        }

        var items = await query
            .Select(u => new UsuarioDto
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                Email = u.Email,
                Celular = u.Celular,
                FechaNacimiento = u.FechaNacimiento,
                Genero = u.Genero,
                Estado = u.Estado,
                FechaAlta = u.FechaAlta
            })
            .ToListAsync(cancellationToken);

        total = isPaginated ? total : items.Count;

        return new PagedResult<UsuarioDto>(
            items,
            total,
            isPaginated ? Math.Max(1, page ?? 1) : null,
            isPaginated ? Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize) : null);
    }

    public async Task<UsuarioDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsuario == id, cancellationToken)
            ?? throw new NotFoundException($"El usuario con ID {id} no existe.");

        return usuario.ToDto();
    }

    public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto, CancellationToken cancellationToken)
    {
        ValidateBirthDate(dto.FechaNacimiento);

        var email = NormalizeEmail(dto.Email);

        var emailExists = await db.Usuarios
            .AnyAsync(u => u.Email.ToLower() == email, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException("Ya existe un usuario con ese email.");
        }

        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Email = email,
            Celular = string.IsNullOrWhiteSpace(dto.Celular) ? null : dto.Celular.Trim(),
            FechaNacimiento = dto.FechaNacimiento,
            Genero = dto.Genero,
            // Se conserva el comportamiento historico: el usuario nace inactivo
            // y se activa con PATCH /api/usuarios/{id}/activar.
            Estado = 0
        };

        db.Usuarios.Add(usuario);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("No fue posible crear el usuario. Verifique que el email no este duplicado.");
        }

        return usuario.ToDto();
    }

    public async Task UpdateAsync(int id, UsuarioUpdateDto dto, CancellationToken cancellationToken)
    {
        ValidateBirthDate(dto.FechaNacimiento);

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == id, cancellationToken)
            ?? throw new NotFoundException($"El usuario con ID {id} no existe.");

        var email = NormalizeEmail(dto.Email);

        var emailExists = await db.Usuarios
            .AnyAsync(u => u.IdUsuario != id && u.Email.ToLower() == email, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException("Ya existe otro usuario con ese email.");
        }

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Email = email;
        usuario.Celular = string.IsNullOrWhiteSpace(dto.Celular) ? null : dto.Celular.Trim();
        usuario.FechaNacimiento = dto.FechaNacimiento;
        usuario.Genero = dto.Genero;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("No fue posible actualizar el usuario. Verifique los datos enviados.");
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == id, cancellationToken)
            ?? throw new NotFoundException($"El usuario con ID {id} no existe.");

        if (usuario.Estado == 1)
        {
            throw new ConflictException("El usuario se encuentra activo y no puede ser eliminado.");
        }

        var detalle = await db.UsuarioDetalles
            .FirstOrDefaultAsync(d => d.IdUsuario == id, cancellationToken);

        if (detalle is not null)
        {
            db.UsuarioDetalles.Remove(detalle);
        }

        db.Usuarios.Remove(usuario);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("No fue posible eliminar el usuario porque tiene registros relacionados.");
        }
    }

    public async Task<UsuarioDto> ActivateAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == id, cancellationToken)
            ?? throw new NotFoundException($"El usuario con ID {id} no existe.");

        if (usuario.Estado == 1)
        {
            throw new ConflictException($"La cuenta de {usuario.Nombre} ya esta activa.");
        }

        usuario.Estado = 1;
        await db.SaveChangesAsync(cancellationToken);

        return usuario.ToDto();
    }

    public async Task<UsuarioDto> DeactivateAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == id, cancellationToken)
            ?? throw new NotFoundException($"El usuario con ID {id} no existe.");

        if (usuario.Estado == 0)
        {
            throw new ConflictException($"La cuenta de {usuario.Nombre} ya esta inactiva.");
        }

        usuario.Estado = 0;
        await db.SaveChangesAsync(cancellationToken);

        return usuario.ToDto();
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static void ValidateBirthDate(DateTime fechaNacimiento)
    {
        if (fechaNacimiento == default)
        {
            throw new BadRequestException("La fecha de nacimiento es obligatoria.");
        }

        if (fechaNacimiento.Date > DateTime.UtcNow.Date)
        {
            throw new BadRequestException("La fecha de nacimiento no puede ser futura.");
        }
    }
}
