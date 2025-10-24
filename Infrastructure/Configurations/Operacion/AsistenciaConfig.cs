using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Operacion;

public class AsistenciaConfig : IEntityTypeConfiguration<Asistencia>
{
    public void Configure(EntityTypeBuilder<Asistencia> builder)
    {
builder.ToTable("Asistencia", "dbo");

        builder.HasKey(a => a.AsistenciaId);

        builder.Property(a => a.Fecha)
     .HasColumnType("date");

        builder.Property(a => a.Estado)
            .HasConversion<byte>();

        builder.Property(a => a.Observacion)
.HasMaxLength(300);

        builder.Property(a => a.RowVersion)
   .IsRowVersion();

        // Índice único
        builder.HasIndex(a => new { a.ActividadId, a.ParticipanteId, a.Fecha })
.IsUnique()
     .HasFilter("[IsDeleted] = 0")
.HasDatabaseName("UX_Asistencia_Act_Part_Fecha");

// Índice adicional
        builder.HasIndex(a => new { a.ParticipanteId, a.Fecha })
      .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Asistencia_Participante_Fecha");
    }
}
