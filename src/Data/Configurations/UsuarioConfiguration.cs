using ApiUsuario.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiUsuario.Data.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");
        builder.HasKey(u => u.IdUsuario);

        builder.Property(u => u.IdUsuario).HasColumnName("id_usuario");
        builder.Property(u => u.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").IsRequired();
        builder.Property(u => u.Celular).HasColumnName("celular");
        builder.Property(u => u.FechaNacimiento).HasColumnName("fecha_nacimiento");
        builder.Property(u => u.Genero).HasColumnName("genero");
        builder.Property(u => u.Estado).HasColumnName("estado");
        builder.Property(u => u.FechaAlta)
            .HasColumnName("fecha_alta")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Evita correos duplicados. En bases existentes revise duplicados antes
        // de aplicar la migracion correspondiente.
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ux_usuario_email");
    }
}
