using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Operacion;

public class ParticipanteConfig : IEntityTypeConfiguration<Participante>
{
    public void Configure(EntityTypeBuilder<Participante> builder)
    {
        builder.ToTable("Participante", "dbo");

        builder.HasKey(p => p.ParticipanteId);

        builder.Property(p => p.Estado)
            .HasConversion<byte>();

        builder.Property(p => p.FechaAlta)
            .HasColumnType("date");

        builder.Property(p => p.FechaBaja)
  .HasColumnType("date");

        builder.Property(p => p.Observaciones)
            .HasMaxLength(500);

        builder.Property(p => p.RowVersion)
.IsRowVersion();

    // Relación 1:1 con Persona
        builder.HasOne(p => p.Persona)
         .WithOne(pe => pe.Participante!)
   .HasForeignKey<Participante>(p => p.PersonaId)
      .OnDelete(DeleteBehavior.NoAction);

        // Relaciones
        builder.HasMany(p => p.ActividadParticipantes)
         .WithOne(ap => ap.Participante)
     .HasForeignKey(ap => ap.ParticipanteId)
.OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Asistencias)
    .WithOne(a => a.Participante)
    .HasForeignKey(a => a.ParticipanteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
