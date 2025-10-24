using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POAInstanciaConfig : IEntityTypeConfiguration<POAInstancia>
{
    public void Configure(EntityTypeBuilder<POAInstancia> builder)
    {
 builder.ToTable("POA_Instancia", "dbo");

     builder.HasKey(i => i.InstanciaId);

        builder.Property(i => i.Estado)
   .HasConversion<byte>();

        builder.Property(i => i.Notas)
  .HasMaxLength(500);

        builder.Property(i => i.RowVersion)
    .IsRowVersion();

   // Índice
 builder.HasIndex(i => new { i.ProgramaId, i.PeriodoAnio, i.PeriodoMes })
    .HasFilter("[IsDeleted] = 0")
 .HasDatabaseName("IX_PInst_Programa_Periodo");

     // Relaciones
        builder.HasMany(i => i.Valores)
    .WithOne(v => v.Instancia)
   .HasForeignKey(v => v.InstanciaId)
      .OnDelete(DeleteBehavior.Cascade);

 builder.HasMany(i => i.Archivos)
     .WithOne(a => a.Instancia)
 .HasForeignKey(a => a.InstanciaId)
   .OnDelete(DeleteBehavior.Cascade);
    }
}
