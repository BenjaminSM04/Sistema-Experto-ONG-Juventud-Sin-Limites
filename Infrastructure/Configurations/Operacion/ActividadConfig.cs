using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Operacion;

public class ActividadConfig : IEntityTypeConfiguration<Actividad>
{
    public void Configure(EntityTypeBuilder<Actividad> builder)
    {
        builder.ToTable("Actividad", "dbo");

        builder.HasKey(a => a.ActividadId);

  builder.Property(a => a.Titulo)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(a => a.Descripcion)
    .HasMaxLength(1000);

        builder.Property(a => a.FechaInicio)
 .HasColumnType("datetime2(0)");

        builder.Property(a => a.FechaFin)
            .HasColumnType("datetime2(0)");

        builder.Property(a => a.Lugar)
   .HasMaxLength(120);

        builder.Property(a => a.Tipo)
  .HasConversion<byte>();

        builder.Property(a => a.Estado)
 .HasConversion<byte>();

      builder.Property(a => a.RowVersion)
         .IsRowVersion();

        // Índice
        builder.HasIndex(a => new { a.ProgramaId, a.FechaInicio })
            .HasFilter("[IsDeleted] = 0")
   .HasDatabaseName("IX_Actividad_Programa_Fecha");

        // Relaciones
        builder.HasMany(a => a.ActividadParticipantes)
            .WithOne(ap => ap.Actividad)
   .HasForeignKey(ap => ap.ActividadId)
   .OnDelete(DeleteBehavior.Cascade);

   builder.HasMany(a => a.Asistencias)
 .WithOne(asi => asi.Actividad)
    .HasForeignKey(asi => asi.ActividadId)
        .OnDelete(DeleteBehavior.Cascade);

 builder.HasMany(a => a.Evidencias)
     .WithOne(e => e.Actividad)
          .HasForeignKey(e => e.ActividadId)
    .OnDelete(DeleteBehavior.Cascade);
    }
}
