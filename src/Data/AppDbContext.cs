using api.Models;
using api_usuario.Models;
using Microsoft.EntityFrameworkCore;

namespace api_usuario.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; } // 👈 agregado
        public DbSet<UsuarioDetalle> UsuarioDetalles { get; set; } // 👈 agregado nuevo

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de Usuario
            modelBuilder.Entity<Usuario>()
                .ToTable("usuario")
                .HasKey(u => u.IdUsuario);

            modelBuilder.Entity<Usuario>().Property(u => u.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<Usuario>().Property(u => u.Nombre).HasColumnName("nombre");
            modelBuilder.Entity<Usuario>().Property(u => u.Email).HasColumnName("email");
            modelBuilder.Entity<Usuario>().Property(u => u.Celular).HasColumnName("celular");
            modelBuilder.Entity<Usuario>().Property(u => u.FechaNacimiento).HasColumnName("fecha_nacimiento");
            modelBuilder.Entity<Usuario>().Property(u => u.Genero).HasColumnName("genero");
            modelBuilder.Entity<Usuario>().Property(u => u.Estado).HasColumnName("estado");
            modelBuilder.Entity<Usuario>().Property(u => u.FechaAlta).HasColumnName("fecha_alta").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

            // Configuración de Departamento
            modelBuilder.Entity<Departamento>()
                .ToTable("departamentos") // 👈 nombre exacto en la BD
                .HasKey(d => d.IdDepartamento);

            modelBuilder.Entity<Departamento>().Property(d => d.IdDepartamento).HasColumnName("id_departamento");
            modelBuilder.Entity<Departamento>().Property(d => d.NombreDepartamento).HasColumnName("nombre_departamento");

            // Configuración de Ciudad
            modelBuilder.Entity<Ciudad>()
                .ToTable("ciudades") // 👈 nombre exacto en la BD
                .HasKey(c => c.IdCiudad);

            modelBuilder.Entity<Ciudad>().Property(c => c.IdCiudad).HasColumnName("id_ciudad");
            modelBuilder.Entity<Ciudad>().Property(c => c.NombreCiudad).HasColumnName("nombre_ciudad");
            modelBuilder.Entity<Ciudad>().Property(c => c.IdDepartamento).HasColumnName("id_departamento");

            // Configuración de UsuarioDetalle
            modelBuilder.Entity<UsuarioDetalle>()
                .ToTable("usuario_detalle") // 👈 nombre exacto en la BD
                .HasKey(ud => ud.IdUsuario);

            modelBuilder.Entity<UsuarioDetalle>().Property(ud => ud.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<UsuarioDetalle>().Property(ud => ud.IdDepartamento).HasColumnName("id_departamento");
            modelBuilder.Entity<UsuarioDetalle>().Property(ud => ud.IdCiudad).HasColumnName("id_ciudad");
            modelBuilder.Entity<UsuarioDetalle>().Property(ud => ud.PasswordHash).HasColumnName("password_hash");
            modelBuilder.Entity<UsuarioDetalle>().Property(ud => ud.FechaUltimaActualizacion).HasColumnName("fecha_ultima_actualizacion")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relaciones
            modelBuilder.Entity<UsuarioDetalle>()
                .HasOne<Usuario>() // Relación con Usuario
                .WithOne()
                .HasForeignKey<UsuarioDetalle>(ud => ud.IdUsuario);

            modelBuilder.Entity<UsuarioDetalle>()
                .HasOne<Departamento>()
                .WithMany()
                .HasForeignKey(ud => ud.IdDepartamento);

            modelBuilder.Entity<UsuarioDetalle>()
                .HasOne<Ciudad>()
                .WithMany()
                .HasForeignKey(ud => ud.IdCiudad);
        }
    }
}