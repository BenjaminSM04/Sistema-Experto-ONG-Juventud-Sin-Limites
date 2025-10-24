using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Operacion;

public class EvidenciaActividadConfig : IEntityTypeConfiguration<EvidenciaActividad>
{
    public void Configure(EntityTypeBuilder<EvidenciaActividad> builder)
    {
  builder.ToTable("EvidenciaActividad", "dbo");

        builder.HasKey(e => e.EvidenciaId);

        builder.Property(e => e.Tipo)
         .HasConversion<byte>();

        builder.Property(e => e.ArchivoPath)
.IsRequired()
    .HasMaxLength(300);

        builder.Property(e => e.SubidoEn)
     .HasColumnType("datetime2(0)");

        builder.Property(e => e.RowVersion)
      .IsRowVersion();

  // Índice
 builder.HasIndex(e => new { e.ActividadId, e.Tipo })
            .HasFilter("[IsDeleted] = 0")
        .HasDatabaseName("IX_Evidencia_Actividad");
  }
}
