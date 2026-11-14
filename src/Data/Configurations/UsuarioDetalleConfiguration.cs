using ApiUsuario.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiUsuario.Data.Configurations;

public sealed class UsuarioDetalleConfiguration : IEntityTypeConfiguration<UsuarioDetalle>
{
    public void Configure(EntityTypeBuilder<UsuarioDetalle> builder)
    {
        builder.ToTable("usuario_detalle");
        builder.HasKey(ud => ud.IdUsuario);

        builder.Property(ud => ud.IdUsuario).HasColumnName("id_usuario");
        builder.Property(ud => ud.IdDepartamento).HasColumnName("id_departamento");
        builder.Property(ud => ud.IdCiudad).HasColumnName("id_ciudad");
        builder.Property(ud => ud.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(ud => ud.FechaUltimaActualizacion)
            .HasColumnName("fecha_ultima_actualizacion")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(ud => ud.IdDepartamento).HasDatabaseName("ix_usuario_detalle_id_departamento");
        builder.HasIndex(ud => ud.IdCiudad).HasDatabaseName("ix_usuario_detalle_id_ciudad");

        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<UsuarioDetalle>(ud => ud.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Departamento>()
            .WithMany()
            .HasForeignKey(ud => ud.IdDepartamento)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Ciudad>()
            .WithMany()
            .HasForeignKey(ud => ud.IdCiudad)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
