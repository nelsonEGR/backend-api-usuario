using System.Reflection;
using ApiUsuario.Controllers;
using ApiUsuario.Security;
using Microsoft.AspNetCore.Authorization;

namespace ApiUsuario.Tests.Security;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public void Controladores_RequierenAutenticacion()
    {
        Type[] controllers =
        [
            typeof(UsuariosController),
            typeof(UsuarioDetallesController),
            typeof(DepartamentosController),
            typeof(CiudadesController)
        ];

        foreach (var controller in controllers)
        {
            Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
        }
    }

    [Fact]
    public void EndpointsAdministrativos_RequierenPoliticaAdmin()
    {
        AssertAdminPolicy<UsuarioDetallesController>(
            nameof(UsuarioDetallesController.AsignarPassword));
        AssertAdminPolicy<UsuarioDetallesController>(
            nameof(UsuarioDetallesController.ResetPassword));
    }

    private static void AssertAdminPolicy<T>(string methodName)
    {
        var method = typeof(T).GetMethod(methodName);
        Assert.NotNull(method);

        var attribute = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(Policies.Admin, attribute!.Policy);
    }
}
