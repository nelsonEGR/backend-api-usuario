using System.ComponentModel.DataAnnotations;
using ApiUsuario.Dtos;

namespace ApiUsuario.Tests.Validation;

public sealed class DtoValidationTests
{
    [Fact]
    public void UsuarioCreateDto_ConEmailYGeneroInvalidos_NoEsValido()
    {
        var dto = new UsuarioCreateDto
        {
            Nombre = "Ana",
            Email = "no-es-un-email",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Genero = 'X'
        };

        var results = Validate(dto);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(UsuarioCreateDto.Email)));
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(UsuarioCreateDto.Genero)));
    }

    [Fact]
    public void UsuarioCreateDto_Valido_EsValido()
    {
        var dto = new UsuarioCreateDto
        {
            Nombre = "Ana",
            Email = "ana@example.com",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Genero = 'F'
        };

        Assert.Empty(Validate(dto));
    }

    [Fact]
    public void PasswordCorta_NoEsValida()
    {
        var dto = new UsuarioPasswordDto
        {
            IdUsuario = 1,
            Password = "corta"
        };

        var results = Validate(dto);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(UsuarioPasswordDto.Password)));
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true);

        return results;
    }
}
