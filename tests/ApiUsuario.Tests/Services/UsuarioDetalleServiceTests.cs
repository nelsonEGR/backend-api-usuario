using ApiUsuario.Common.Exceptions;
using ApiUsuario.Dtos;
using ApiUsuario.Models;
using ApiUsuario.Security;
using ApiUsuario.Services;

namespace ApiUsuario.Tests.Services;

public sealed class UsuarioDetalleServiceTests
{
    private readonly IPasswordHasher _hasher = new BCryptPasswordHasher();

    [Fact]
    public async Task AssignPasswordAsync_GuardaHashYNoLaContrasena()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(NewUser(1));
        await db.SaveChangesAsync();

        var service = new UsuarioDetalleService(db, _hasher);

        await service.AssignPasswordAsync(new UsuarioPasswordDto
        {
            IdUsuario = 1,
            Password = "Password123"
        }, CancellationToken.None);

        var detalle = await db.UsuarioDetalles.FindAsync(1);

        Assert.NotNull(detalle);
        Assert.NotEqual("Password123", detalle!.PasswordHash);
        Assert.True(_hasher.Verify("Password123", detalle.PasswordHash));
    }

    [Fact]
    public async Task AssignPasswordAsync_SiYaExiste_LanzaConflicto()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(NewUser(1));
        db.UsuarioDetalles.Add(new UsuarioDetalle
        {
            IdUsuario = 1,
            PasswordHash = _hasher.Hash("Password123")
        });
        await db.SaveChangesAsync();

        var service = new UsuarioDetalleService(db, _hasher);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.AssignPasswordAsync(new UsuarioPasswordDto
            {
                IdUsuario = 1,
                Password = "OtraClave123"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task ChangePasswordAsync_ContrasenaActualIncorrecta_LanzaBadRequest()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(NewUser(1));
        db.UsuarioDetalles.Add(new UsuarioDetalle
        {
            IdUsuario = 1,
            PasswordHash = _hasher.Hash("Password123")
        });
        await db.SaveChangesAsync();

        var service = new UsuarioDetalleService(db, _hasher);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.ChangePasswordAsync(new UsuarioCambioPasswordDto
            {
                IdUsuario = 1,
                PasswordActual = "Incorrecta123",
                PasswordNueva = "NuevaClave123"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateLocationAsync_CiudadNoPerteneceAlDepartamento_LanzaBadRequest()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(NewUser(1));
        db.Departamentos.Add(new Departamento
        {
            IdDepartamento = 1,
            NombreDepartamento = "Antioquia"
        });
        db.Departamentos.Add(new Departamento
        {
            IdDepartamento = 2,
            NombreDepartamento = "Cundinamarca"
        });
        db.Ciudades.Add(new Ciudad
        {
            IdCiudad = 10,
            NombreCiudad = "Medellin",
            IdDepartamento = 1
        });
        db.UsuarioDetalles.Add(new UsuarioDetalle
        {
            IdUsuario = 1,
            PasswordHash = "hash"
        });
        await db.SaveChangesAsync();

        var service = new UsuarioDetalleService(db, _hasher);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateLocationAsync(1, new UsuarioUbicacionDto
            {
                IdCiudad = 10,
                IdDepartamento = 2
            }, CancellationToken.None));
    }

    [Fact]
    public async Task ResetPasswordAsync_SinDetalle_LanzaNotFound()
    {
        await using var db = TestDb.Create();
        var service = new UsuarioDetalleService(db, _hasher);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ResetPasswordAsync(new UsuarioResetPasswordAdminDto
            {
                IdUsuario = 99,
                PasswordNueva = "NuevaClave123"
            }, CancellationToken.None));
    }

    private static Usuario NewUser(int id) => new()
    {
        IdUsuario = id,
        Nombre = "Usuario",
        Email = $"usuario{id}@example.com",
        FechaNacimiento = new DateTime(1990, 1, 1),
        Genero = 'M',
        Estado = 0
    };
}
