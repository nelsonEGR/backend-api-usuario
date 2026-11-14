using ApiUsuario.Configuration;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ApiUsuario.Tests.Configuration;

public sealed class ConnectionStringResolverTests
{
    [Fact]
    public void Resolve_ConUriPostgresql_ConvierteAFormatoNpgsql()
    {
        var configuration = Build(
            "postgresql://usuario:clave@host.example.com/neondb?sslmode=require&channel_binding=require");

        var result = ConnectionStringResolver.Resolve(configuration);
        var builder = new NpgsqlConnectionStringBuilder(result);

        Assert.Equal("host.example.com", builder.Host);
        Assert.Equal(5432, builder.Port);
        Assert.Equal("neondb", builder.Database);
        Assert.Equal("usuario", builder.Username);
        Assert.Equal("clave", builder.Password);
        Assert.Equal(SslMode.Require, builder.SslMode);
        Assert.Equal(ChannelBinding.Require, builder.ChannelBinding);
    }

    [Fact]
    public void Resolve_ConUriPostgresqlSinPuerto_Usa5432()
    {
        var configuration = Build("postgres://usuario:clave@host.example.com/neondb");

        var result = ConnectionStringResolver.Resolve(configuration);
        var builder = new NpgsqlConnectionStringBuilder(result);

        Assert.Equal(5432, builder.Port);
        Assert.Equal(SslMode.Require, builder.SslMode);
    }

    [Fact]
    public void Resolve_ConCadenaNpgsql_LaDevuelveIgual()
    {
        const string original =
            "Host=localhost;Port=5432;Database=test;Username=postgres;Password=postgres";

        var result = ConnectionStringResolver.Resolve(Build(original));

        Assert.Equal(original, result);
    }

    [Fact]
    public void Resolve_SinCadena_Throws()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<InvalidOperationException>(() =>
            ConnectionStringResolver.Resolve(configuration));
    }

    private static IConfiguration Build(string connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            })
            .Build();
}
