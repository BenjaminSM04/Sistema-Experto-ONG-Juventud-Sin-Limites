using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POACampoConfig : IEntityTypeConfiguration<POACampo>
{
    public void Configure(EntityTypeBuilder<POACampo> builder)
    {
 builder.ToTable("POA_Campo", "dbo");

        builder.HasKey(c => c.CampoId);

        builder.Property(c => c.Clave)
     .IsRequired()
     .HasMaxLength(80);

        builder.Property(c => c.Etiqueta)
   .IsRequired()
     .HasMaxLength(200);

        builder.Property(c => c.TipoDato)
     .HasConversion<byte>();

   builder.Property(c => c.Alcance)
       .HasConversion<byte>();

  builder.Property(c => c.Unidad)
            .HasMaxLength(30);

    builder.Property(c => c.RowVersion)
  .IsRowVersion();

 // Índice único
        builder.HasIndex(c => new { c.PlantillaId, c.Clave })
.IsUnique()
    .HasFilter("[IsDeleted] = 0")
        .HasDatabaseName("UX_PCampo_Plantilla_Clave");

        // Relaciones
 builder.HasMany(c => c.Opciones)
     .WithOne(o => o.Campo)
    .HasForeignKey(o => o.CampoId)
            .OnDelete(DeleteBehavior.Cascade);

   builder.HasMany(c => c.Validaciones)
   .WithOne(v => v.Campo)
  .HasForeignKey(v => v.CampoId)
   .OnDelete(DeleteBehavior.Cascade);

builder.HasMany(c => c.Valores)
 .WithOne(v => v.Campo)
            .HasForeignKey(v => v.CampoId)
 .OnDelete(DeleteBehavior.Cascade);
    }
}
