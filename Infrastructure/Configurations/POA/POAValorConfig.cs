using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POAValorConfig : IEntityTypeConfiguration<POAValor>
{
    public void Configure(EntityTypeBuilder<POAValor> builder)
    {
     builder.ToTable("POA_Valor", "dbo");

   builder.HasKey(v => v.ValorId);

   builder.Property(v => v.ValorDecimal)
   .HasPrecision(18, 4);

   builder.Property(v => v.ValorFecha)
    .HasColumnType("date");

        builder.Property(v => v.RowVersion)
     .IsRowVersion();

        // Índices
  builder.HasIndex(v => new { v.InstanciaId, v.CampoId })
 .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_PValor_Target_Inst");

   builder.HasIndex(v => v.ProgramaId)
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_PValor_Programa");

 builder.HasIndex(v => v.ActividadId)
     .HasFilter("[IsDeleted] = 0")
     .HasDatabaseName("IX_PValor_Actividad");

   builder.HasIndex(v => v.ParticipanteId)
        .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_PValor_Participante");

        // Relaciones opcionales
        builder.HasOne(v => v.Programa)
      .WithMany()
   .HasForeignKey(v => v.ProgramaId)
  .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(v => v.Actividad)
   .WithMany()
       .HasForeignKey(v => v.ActividadId)
    .OnDelete(DeleteBehavior.NoAction);

  builder.HasOne(v => v.Participante)
   .WithMany()
    .HasForeignKey(v => v.ParticipanteId)
        .OnDelete(DeleteBehavior.NoAction);
    }
}
