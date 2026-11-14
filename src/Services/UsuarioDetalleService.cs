using ApiUsuario.Common.Exceptions;
using ApiUsuario.Data;
using ApiUsuario.Dtos;
using ApiUsuario.Models;
using ApiUsuario.Security;
using Microsoft.EntityFrameworkCore;

namespace ApiUsuario.Services;

public sealed class UsuarioDetalleService(AppDbContext db, IPasswordHasher passwordHasher)
    : IUsuarioDetalleService
{
    public async Task AssignPasswordAsync(UsuarioPasswordDto dto, CancellationToken cancellationToken)
    {
        ValidatePassword(dto.Password);

        var usuarioExists = await db.Usuarios
            .AnyAsync(u => u.IdUsuario == dto.IdUsuario, cancellationToken);

        if (!usuarioExists)
        {
            throw new NotFoundException("El usuario no existe.");
        }

        var detailExists = await db.UsuarioDetalles
            .AnyAsync(d => d.IdUsuario == dto.IdUsuario, cancellationToken);

        if (detailExists)
        {
            throw new ConflictException("El usuario ya tiene contrasena asignada.");
        }

        var detalle = new UsuarioDetalle
        {
            IdUsuario = dto.IdUsuario,
            IdDepartamento = null,
            IdCiudad = null,
            PasswordHash = passwordHasher.Hash(dto.Password),
            FechaUltimaActualizacion = DateTime.UtcNow
        };

        db.UsuarioDetalles.Add(detalle);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("No fue posible asignar la contrasena. Intente nuevamente.");
        }
    }

    public async Task ChangePasswordAsync(
        UsuarioCambioPasswordDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.PasswordActual))
        {
            throw new BadRequestException("La contrasena actual es obligatoria.");
        }

        ValidatePassword(dto.PasswordNueva);

        var detalle = await db.UsuarioDetalles
            .FirstOrDefaultAsync(d => d.IdUsuario == dto.IdUsuario, cancellationToken)
            ?? throw new BadRequestException("El usuario no tiene contrasena asignada.");

        if (!passwordHasher.Verify(dto.PasswordActual, detalle.PasswordHash))
        {
            throw new BadRequestException("La contrasena actual es incorrecta.");
        }

        detalle.PasswordHash = passwordHasher.Hash(dto.PasswordNueva);
        detalle.FechaUltimaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(
        UsuarioResetPasswordAdminDto dto,
        CancellationToken cancellationToken)
    {
        ValidatePassword(dto.PasswordNueva);

        var detalle = await db.UsuarioDetalles
            .FirstOrDefaultAsync(d => d.IdUsuario == dto.IdUsuario, cancellationToken)
            ?? throw new NotFoundException("El usuario no tiene detalle registrado.");

        detalle.PasswordHash = passwordHasher.Hash(dto.PasswordNueva);
        detalle.FechaUltimaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLocationAsync(
        int idUsuario,
        UsuarioUbicacionDto dto,
        CancellationToken cancellationToken)
    {
        var detalle = await db.UsuarioDetalles
            .FirstOrDefaultAsync(d => d.IdUsuario == idUsuario, cancellationToken)
            ?? throw new NotFoundException($"No existe un registro de detalle para el usuario con ID {idUsuario}.");

        var ciudad = await db.Ciudades
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCiudad == dto.IdCiudad, cancellationToken)
            ?? throw new NotFoundException($"La ciudad con ID {dto.IdCiudad} no existe.");

        var departamentoExists = await db.Departamentos
            .AnyAsync(d => d.IdDepartamento == dto.IdDepartamento, cancellationToken);

        if (!departamentoExists)
        {
            throw new NotFoundException($"El departamento con ID {dto.IdDepartamento} no existe.");
        }

        if (ciudad.IdDepartamento != dto.IdDepartamento)
        {
            throw new BadRequestException(
                $"La ciudad con ID {dto.IdCiudad} no pertenece al departamento con ID {dto.IdDepartamento}.");
        }

        detalle.IdCiudad = dto.IdCiudad;
        detalle.IdDepartamento = dto.IdDepartamento;
        detalle.FechaUltimaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 72)
        {
            throw new BadRequestException("La contrasena debe tener entre 8 y 72 caracteres.");
        }
    }
}
