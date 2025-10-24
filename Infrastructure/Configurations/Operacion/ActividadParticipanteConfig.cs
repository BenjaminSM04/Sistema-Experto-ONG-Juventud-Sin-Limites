using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Operacion;

public class ActividadParticipanteConfig : IEntityTypeConfiguration<ActividadParticipante>
{
    public void Configure(EntityTypeBuilder<ActividadParticipante> builder)
    {
   builder.ToTable("ActividadParticipante", "dbo");

        // PK compuesta
        builder.HasKey(ap => new { ap.ActividadId, ap.ParticipanteId });

   builder.Property(ap => ap.Rol)
   .HasConversion<byte>();

        builder.Property(ap => ap.Estado)
   .HasConversion<byte>();

        builder.Property(ap => ap.RowVersion)
   .IsRowVersion();

        // Índice
        builder.HasIndex(ap => ap.ParticipanteId)
  .HasFilter("[IsDeleted] = 0")
         .HasDatabaseName("IX_AP_Participante");
    }
}
