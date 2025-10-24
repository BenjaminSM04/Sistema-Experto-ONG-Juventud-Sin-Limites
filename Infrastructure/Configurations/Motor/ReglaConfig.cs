using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Motor;

public class ReglaConfig : IEntityTypeConfiguration<Regla>
{
    public void Configure(EntityTypeBuilder<Regla> builder)
    {
      builder.ToTable("Regla", "dbo");

  builder.HasKey(r => r.ReglaId);

  builder.Property(r => r.Clave)
         .IsRequired()
            .HasMaxLength(80);

  builder.Property(r => r.Nombre)
    .IsRequired()
        .HasMaxLength(200);

  builder.Property(r => r.Descripcion)
     .HasMaxLength(500);

  builder.Property(r => r.Severidad)
       .HasConversion<byte>();

   builder.Property(r => r.Objetivo)
     .HasConversion<byte>();

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

   // Índice único
        builder.HasIndex(r => r.Clave)
     .IsUnique()
 .HasFilter("[IsDeleted] = 0")
     .HasDatabaseName("UX_Regla_Clave_Unique");

        // Relaciones
        builder.HasMany(r => r.Parametros)
    .WithOne(p => p.Regla)
   .HasForeignKey(p => p.ReglaId)
       .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(r => r.ParametroOverrides)
       .WithOne(po => po.Regla)
    .HasForeignKey(po => po.ReglaId)
     .OnDelete(DeleteBehavior.Cascade);

  builder.HasMany(r => r.Alertas)
  .WithOne(a => a.Regla)
      .HasForeignKey(a => a.ReglaId)
   .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(r => r.Matches)
 .WithOne(m => m.Regla)
            .HasForeignKey(m => m.ReglaId)
      .OnDelete(DeleteBehavior.NoAction);
    }
}
