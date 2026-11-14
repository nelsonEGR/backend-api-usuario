using ApiUsuario.Common.Exceptions;
using ApiUsuario.Controllers;
using ApiUsuario.Data;
using ApiUsuario.Dtos;
using ApiUsuario.Models;
using ApiUsuario.Security;
using ApiUsuario.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Tests.Controllers;

public sealed class UsuarioDetallesControllerTests
{
    private readonly IPasswordHasher _hasher = new BCryptPasswordHasher();

    [Fact]
    public async Task AsignarPassword_Valido_DevuelveOk()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(NewUser(1));
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.AsignarPassword(new UsuarioPasswordDto
        {
            IdUsuario = 1,
            Password = "Password123"
        }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, await db.UsuarioDetalles.CountAsync());
    }

    [Fact]
    public async Task ResetPassword_SinDetalle_LanzaNotFound()
    {
        await using var db = TestDb.Create();
        var controller = CreateController(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            controller.ResetPassword(new UsuarioResetPasswordAdminDto
            {
                IdUsuario = 99,
                PasswordNueva = "NuevaClave123"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task CambiarPassword_ActualIncorrecta_LanzaBadRequest()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(NewUser(1));
        db.UsuarioDetalles.Add(new UsuarioDetalle
        {
            IdUsuario = 1,
            PasswordHash = _hasher.Hash("Password123")
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            controller.CambiarPassword(new UsuarioCambioPasswordDto
            {
                IdUsuario = 1,
                PasswordActual = "Incorrecta123",
                PasswordNueva = "NuevaClave123"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task ActualizarUbicacion_Valida_DevuelveOk()
    {
        await using var db = TestDb.Create();
        SeedLocation(db);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.ActualizarUbicacionUsuario(1, new UsuarioUbicacionDto
        {
            IdCiudad = 10,
            IdDepartamento = 1
        }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var detalle = await db.UsuarioDetalles.FindAsync(1);
        Assert.Equal(10, detalle!.IdCiudad);
        Assert.Equal(1, detalle.IdDepartamento);
    }

    private UsuarioDetallesController CreateController(AppDbContext db)
    {
        var controller = new UsuarioDetallesController(new UsuarioDetalleService(db, _hasher));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    private static void SeedLocation(AppDbContext db)
    {
        db.Usuarios.Add(NewUser(1));
        db.Departamentos.Add(new Departamento { IdDepartamento = 1, NombreDepartamento = "Antioquia" });
        db.Ciudades.Add(new Ciudad { IdCiudad = 10, NombreCiudad = "Medellin", IdDepartamento = 1 });
        db.UsuarioDetalles.Add(new UsuarioDetalle { IdUsuario = 1, PasswordHash = "hash" });
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
