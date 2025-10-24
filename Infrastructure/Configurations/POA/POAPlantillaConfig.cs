using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POAPlantillaConfig : IEntityTypeConfiguration<POAPlantilla>
{
    public void Configure(EntityTypeBuilder<POAPlantilla> builder)
  {
        builder.ToTable("POA_Plantilla", "dbo");

        builder.HasKey(p => p.PlantillaId);

        builder.Property(p => p.Estado)
          .HasConversion<byte>();

        builder.Property(p => p.VigenteDesde)
        .HasColumnType("date");

        builder.Property(p => p.VigenteHasta)
       .HasColumnType("date");

        builder.Property(p => p.RowVersion)
          .IsRowVersion();

        // Índice único
        builder.HasIndex(p => new { p.ProgramaId, p.Version })
            .IsUnique()
    .HasFilter("[IsDeleted] = 0")
   .HasDatabaseName("UX_Plantilla_Programa_Version");

        // Relaciones
        builder.HasMany(p => p.Secciones)
   .WithOne(s => s.Plantilla)
   .HasForeignKey(s => s.PlantillaId)
   .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Campos)
        .WithOne(c => c.Plantilla)
        .HasForeignKey(c => c.PlantillaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Instancias)
         .WithOne(i => i.Plantilla)
  .HasForeignKey(i => i.PlantillaId)
          .OnDelete(DeleteBehavior.NoAction);
    }
}
