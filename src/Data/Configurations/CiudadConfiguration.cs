using ApiUsuario.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiUsuario.Data.Configurations;

public sealed class CiudadConfiguration : IEntityTypeConfiguration<Ciudad>
{
    public void Configure(EntityTypeBuilder<Ciudad> builder)
    {
        builder.ToTable("ciudades");
        builder.HasKey(c => c.IdCiudad);

        builder.Property(c => c.IdCiudad).HasColumnName("id_ciudad");
        builder.Property(c => c.NombreCiudad).HasColumnName("nombre_ciudad").IsRequired();
        builder.Property(c => c.IdDepartamento).HasColumnName("id_departamento");

        builder.HasIndex(c => c.IdDepartamento).HasDatabaseName("ix_ciudades_id_departamento");

        builder.HasOne(c => c.Departamento)
            .WithMany()
            .HasForeignKey(c => c.IdDepartamento)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
