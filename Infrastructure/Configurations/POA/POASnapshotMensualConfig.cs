using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POASnapshotMensualConfig : IEntityTypeConfiguration<POASnapshotMensual>
{
    public void Configure(EntityTypeBuilder<POASnapshotMensual> builder)
    {
        builder.ToTable("POA_SnapshotMensual", "dbo");

        // PK compuesta
        builder.HasKey(s => new { s.ProgramaId, s.AnioMes });

        builder.Property(s => s.AnioMes)
        .IsRequired()
    .HasMaxLength(7)
            .IsFixedLength();

     builder.Property(s => s.PayloadJson)
       .IsRequired();

     builder.Property(s => s.RowVersion)
  .IsRowVersion();

        // Relación
        builder.HasOne(s => s.Programa)
  .WithMany()
            .HasForeignKey(s => s.ProgramaId)
    .OnDelete(DeleteBehavior.NoAction);
    }
}
