using ApiUsuario.Security;

namespace ApiUsuario.Tests.Security;

public sealed class PasswordHasherTests
{
    private readonly IPasswordHasher _hasher = new BCryptPasswordHasher();

    [Fact]
    public void Hash_NoDevuelveLaContrasenaEnClaro()
    {
        const string password = "Password123";

        var hash = _hasher.Hash(password);

        Assert.NotEqual(password, hash);
        Assert.True(_hasher.Verify(password, hash));
    }

    [Fact]
    public void Verify_ConContrasenaIncorrecta_DevuelveFalse()
    {
        var hash = _hasher.Hash("Password123");

        Assert.False(_hasher.Verify("OtraClave123", hash));
    }

    [Fact]
    public void Hash_UsaSaltDistintoEnCadaLlamada()
    {
        var first = _hasher.Hash("Password123");
        var second = _hasher.Hash("Password123");

        Assert.NotEqual(first, second);
        Assert.True(_hasher.Verify("Password123", first));
        Assert.True(_hasher.Verify("Password123", second));
    }
}
