using ApiUsuario.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiUsuario.Data.Configurations;

public sealed class DepartamentoConfiguration : IEntityTypeConfiguration<Departamento>
{
    public void Configure(EntityTypeBuilder<Departamento> builder)
    {
        builder.ToTable("departamentos");
        builder.HasKey(d => d.IdDepartamento);

        builder.Property(d => d.IdDepartamento).HasColumnName("id_departamento");
        builder.Property(d => d.NombreDepartamento)
            .HasColumnName("nombre_departamento")
            .IsRequired();
    }
}
