using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Programas;

public class ProgramaConfig : IEntityTypeConfiguration<Programa>
{
    public void Configure(EntityTypeBuilder<Programa> builder)
 {
        builder.ToTable("Programa", "dbo");

        builder.HasKey(p => p.ProgramaId);

        builder.Property(p => p.Clave)
            .IsRequired()
   .HasMaxLength(40);

     builder.Property(p => p.Nombre)
     .IsRequired()
 .HasMaxLength(120);

        builder.Property(p => p.Descripcion)
         .HasMaxLength(500);

        builder.Property(p => p.Estado)
     .HasConversion<byte>();

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

 // Índice único filtrado
        builder.HasIndex(p => p.Clave)
            .IsUnique()
    .HasFilter("[IsDeleted] = 0")
     .HasDatabaseName("IX_Programa_Clave_Unique");

        // Relaciones
        builder.HasMany(p => p.UsuarioProgramas)
      .WithOne(up => up.Programa)
  .HasForeignKey(up => up.ProgramaId)
            .OnDelete(DeleteBehavior.Cascade);

  builder.HasMany(p => p.Actividades)
     .WithOne(a => a.Programa)
    .HasForeignKey(a => a.ProgramaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Plantillas)
   .WithOne(pl => pl.Programa)
     .HasForeignKey(pl => pl.ProgramaId)
 .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.Instancias)
     .WithOne(i => i.Programa)
        .HasForeignKey(i => i.ProgramaId)
      .OnDelete(DeleteBehavior.NoAction);
    }
}
