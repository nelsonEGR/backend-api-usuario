using ApiUsuario.Controllers;
using ApiUsuario.Data;
using ApiUsuario.Dtos;
using ApiUsuario.Models;
using ApiUsuario.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Tests.Controllers;

public sealed class CiudadesControllerTests
{
    [Fact]
    public async Task GetCiudades_SinFiltros_DevuelveTodas()
    {
        await using var db = TestDb.Create();
        Seed(db);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.GetCiudades(null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<CiudadDto>>(ok.Value);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetCiudades_FiltrandoPorDepartamento_DevuelveSoloLasSuyas()
    {
        await using var db = TestDb.Create();
        Seed(db);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.GetCiudades(null, 1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<CiudadDto>>(ok.Value);
        Assert.Single(items);
        Assert.Equal(10, items[0].IdCiudad);
    }

    [Fact]
    public async Task GetCiudades_SinCoincidencias_DevuelveListaVacia()
    {
        await using var db = TestDb.Create();
        Seed(db);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.GetCiudades(999, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<CiudadDto>>(ok.Value);
        Assert.Empty(items);
    }

    private static CiudadesController CreateController(AppDbContext db)
    {
        var controller = new CiudadesController(new CiudadService(db));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    private static void Seed(AppDbContext db)
    {
        db.Departamentos.Add(new Departamento { IdDepartamento = 1, NombreDepartamento = "Antioquia" });
        db.Departamentos.Add(new Departamento { IdDepartamento = 2, NombreDepartamento = "Cundinamarca" });
        db.Ciudades.Add(new Ciudad { IdCiudad = 10, NombreCiudad = "Medellin", IdDepartamento = 1 });
        db.Ciudades.Add(new Ciudad { IdCiudad = 20, NombreCiudad = "Bogota", IdDepartamento = 2 });
    }
}
