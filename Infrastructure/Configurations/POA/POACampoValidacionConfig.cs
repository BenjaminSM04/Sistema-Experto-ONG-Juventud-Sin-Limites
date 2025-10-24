using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POACampoValidacionConfig : IEntityTypeConfiguration<POACampoValidacion>
{
    public void Configure(EntityTypeBuilder<POACampoValidacion> builder)
    {
        builder.ToTable("POA_CampoValidacion", "dbo");

  builder.HasKey(v => v.ValidacionId);

        builder.Property(v => v.Tipo)
 .HasConversion<byte>();

        builder.Property(v => v.Parametro)
 .IsRequired()
            .HasMaxLength(400);

        builder.Property(v => v.RowVersion)
.IsRowVersion();
    }
}
