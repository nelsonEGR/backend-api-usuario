using ApiUsuario.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUsuario.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Ciudad> Ciudades => Set<Ciudad>();
    public DbSet<UsuarioDetalle> UsuarioDetalles => Set<UsuarioDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
