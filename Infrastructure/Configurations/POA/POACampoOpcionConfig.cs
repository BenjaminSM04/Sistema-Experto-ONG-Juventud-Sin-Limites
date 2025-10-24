using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POACampoOpcionConfig : IEntityTypeConfiguration<POACampoOpcion>
{
    public void Configure(EntityTypeBuilder<POACampoOpcion> builder)
    {
   builder.ToTable("POA_CampoOpcion", "dbo");

  builder.HasKey(o => o.OpcionId);

 builder.Property(o => o.Valor)
.IsRequired()
  .HasMaxLength(80);

        builder.Property(o => o.Etiqueta)
  .IsRequired()
      .HasMaxLength(120);

        builder.Property(o => o.RowVersion)
 .IsRowVersion();
    }
}
