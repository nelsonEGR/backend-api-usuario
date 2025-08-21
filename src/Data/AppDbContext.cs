using Microsoft.EntityFrameworkCore;
using api_usuario.Models;

namespace api_usuario.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
            modelBuilder.Entity<Usuario>().Property(u => u.FechaAlta).HasColumnName("fecha_alta");
        }
    }
}