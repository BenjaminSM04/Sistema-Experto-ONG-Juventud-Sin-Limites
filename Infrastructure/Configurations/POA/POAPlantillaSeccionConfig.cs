using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POAPlantillaSeccionConfig : IEntityTypeConfiguration<POAPlantillaSeccion>
{
    public void Configure(EntityTypeBuilder<POAPlantillaSeccion> builder)
    {
        builder.ToTable("POA_PlantillaSeccion", "dbo");

        builder.HasKey(s => s.SeccionId);

 builder.Property(s => s.Nombre)
      .IsRequired()
     .HasMaxLength(120);

  builder.Property(s => s.RowVersion)
  .IsRowVersion();

        // Relaciones
  builder.HasMany(s => s.Campos)
   .WithOne(c => c.Seccion)
  .HasForeignKey(c => c.SeccionId)
   .OnDelete(DeleteBehavior.NoAction);
    }
}
