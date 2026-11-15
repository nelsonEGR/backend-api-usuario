using ApiUsuario.Common.Exceptions;
using ApiUsuario.Dtos;
using ApiUsuario.Models;
using ApiUsuario.Services;

namespace ApiUsuario.Tests.Services;

public sealed class UsuarioServiceTests
{
    [Fact]
    public async Task CreateAsync_CreaUsuarioInactivoYNormalizaEmail()
    {
        await using var db = TestDb.Create();
        var service = new UsuarioService(db);

        var result = await service.CreateAsync(new UsuarioCreateDto
        {
            Nombre = "  Ana Perez  ",
            Email = "  Ana@Example.COM  ",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Genero = 'F'
        }, CancellationToken.None);

        Assert.Equal(0, result.Estado);
        Assert.Equal("Ana Perez", result.Nombre);
        Assert.Equal("ana@example.com", result.Email);
        Assert.Equal(1, await db.Usuarios.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_EmailDuplicado_LanzaConflicto()
    {
        await using var db = TestDb.Create();
        var service = new UsuarioService(db);

        await service.CreateAsync(NewUser("ana@example.com"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(NewUser("ANA@example.com"), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_FechaFutura_LanzaBadRequest()
    {
        await using var db = TestDb.Create();
        var service = new UsuarioService(db);

        var dto = NewUser("futuro@example.com");
        dto.FechaNacimiento = DateTime.UtcNow.AddDays(1);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_UsuarioInexistente_LanzaNotFound()
    {
        await using var db = TestDb.Create();
        var service = new UsuarioService(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(99, new UsuarioUpdateDto
            {
                Nombre = "Nadie",
                Email = "nadie@example.com",
                FechaNacimiento = new DateTime(1990, 1, 1),
                Genero = 'M'
            }, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_ConPaginacion_DevuelveTotalYSubconjunto()
    {
        await using var db = TestDb.Create();

        for (var i = 1; i <= 5; i++)
        {
            db.Usuarios.Add(new Usuario
            {
                IdUsuario = i,
                Nombre = $"Usuario {i}",
                Email = $"usuario{i}@example.com",
                FechaNacimiento = new DateTime(1990, 1, 1),
                Genero = 'M',
                Estado = 0
            });
        }

        await db.SaveChangesAsync();

        var service = new UsuarioService(db);
        var result = await service.GetAllAsync(page: 2, pageSize: 2, CancellationToken.None);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.Items[0].IdUsuario);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    private static UsuarioCreateDto NewUser(string email) => new()
    {
        Nombre = "Ana",
        Email = email,
        FechaNacimiento = new DateTime(1990, 1, 1),
        Genero = 'F'
    };
}
