using ApiUsuario.Common.Exceptions;
using ApiUsuario.Controllers;
using ApiUsuario.Data;
using ApiUsuario.Dtos;
using ApiUsuario.Models;
using ApiUsuario.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Tests.Controllers;

public sealed class UsuariosControllerTests
{
    [Fact]
    public async Task GetUsuarios_ConId_DevuelveUsuario()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(NewUser(1, "ana@example.com"));
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.GetUsuarios(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UsuarioDto>(ok.Value);
        Assert.Equal(1, dto.IdUsuario);
    }

    [Fact]
    public async Task GetUsuarios_Paginado_AgregaHeaderXTotalCount()
    {
        await using var db = TestDb.Create();
        for (var i = 1; i <= 5; i++)
        {
            db.Usuarios.Add(NewUser(i, $"usuario{i}@example.com"));
        }

        await db.SaveChangesAsync();

        var controller = CreateController(db, "?page=1&pageSize=2");

        var result = await controller.GetUsuarios(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<UsuarioDto>>(ok.Value);
        Assert.Equal(2, items.Count);
        Assert.Equal("5", controller.Response.Headers["X-Total-Count"].ToString());
    }

    [Fact]
    public async Task GetUsuarios_PaginaInvalida_LanzaBadRequest()
    {
        await using var db = TestDb.Create();
        var controller = CreateController(db, "?page=no-es-numero");

        await Assert.ThrowsAsync<BadRequestException>(() =>
            controller.GetUsuarios(null, CancellationToken.None));
    }

    [Fact]
    public async Task GetUsuarios_SinRegistros_Devuelve204()
    {
        await using var db = TestDb.Create();
        var controller = CreateController(db);

        var result = await controller.GetUsuarios(null, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("0", controller.Response.Headers["X-Total-Count"].ToString());
    }

    [Fact]
    public async Task GetUsuarios_ConIdInexistente_LanzaNotFound()
    {
        await using var db = TestDb.Create();
        var controller = CreateController(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            controller.GetUsuarios(99, CancellationToken.None));
    }

    [Fact]
    public async Task PostUsuario_Valido_DevuelveOk()
    {
        await using var db = TestDb.Create();
        var controller = CreateController(db);

        var result = await controller.PostUsuario(new UsuarioCreateDto
        {
            Nombre = "Ana Perez",
            Email = "ana@example.com",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Genero = 'F'
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<UsuarioDto>(ok.Value);
        Assert.Equal(1, await db.Usuarios.CountAsync());
    }

    [Fact]
    public async Task DeleteUsuario_Activo_LanzaConflicto()
    {
        await using var db = TestDb.Create();
        var usuario = NewUser(1, "activo@example.com");
        usuario.Estado = 1;
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            controller.DeleteUsuario(1, CancellationToken.None));
    }

    [Fact]
    public async Task ActivarUsuario_Inactivo_DevuelveOk()
    {
        await using var db = TestDb.Create();
        db.Usuarios.Add(NewUser(1, "inactivo@example.com"));
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.ActivarUsuario(1, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, (await db.Usuarios.FindAsync(1))!.Estado);
    }

    private static UsuariosController CreateController(AppDbContext db, string? query = null)
    {
        var controller = new UsuariosController(new UsuarioService(db));
        var context = new DefaultHttpContext();

        if (query is not null)
        {
            context.Request.QueryString = new QueryString(query);
        }

        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }

    private static Usuario NewUser(int id, string email) => new()
    {
        IdUsuario = id,
        Nombre = $"Usuario {id}",
        Email = email,
        FechaNacimiento = new DateTime(1990, 1, 1),
        Genero = 'M',
        Estado = 0
    };
}
