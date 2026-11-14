using ApiUsuario.Common.Exceptions;
using ApiUsuario.Controllers;
using ApiUsuario.Data;
using ApiUsuario.Dtos;
using ApiUsuario.Models;
using ApiUsuario.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Tests.Controllers;

public sealed class DepartamentosControllerTests
{
    [Fact]
    public async Task GetDepartamentos_SinId_DevuelveTodos()
    {
        await using var db = TestDb.Create();
        db.Departamentos.Add(new Departamento { IdDepartamento = 1, NombreDepartamento = "Antioquia" });
        db.Departamentos.Add(new Departamento { IdDepartamento = 2, NombreDepartamento = "Cundinamarca" });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.GetDepartamentos(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<DepartamentoDto>>(ok.Value);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetDepartamentos_ConId_DevuelveDepartamento()
    {
        await using var db = TestDb.Create();
        db.Departamentos.Add(new Departamento { IdDepartamento = 1, NombreDepartamento = "Antioquia" });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.GetDepartamentos(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<DepartamentoDto>(ok.Value);
        Assert.Equal("Antioquia", dto.NombreDepartamento);
    }

    [Fact]
    public async Task GetDepartamentos_ConIdInexistente_LanzaNotFound()
    {
        await using var db = TestDb.Create();
        var controller = CreateController(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            controller.GetDepartamentos(99, CancellationToken.None));
    }

    private static DepartamentosController CreateController(AppDbContext db)
    {
        var controller = new DepartamentosController(new DepartamentoService(db));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }
}
