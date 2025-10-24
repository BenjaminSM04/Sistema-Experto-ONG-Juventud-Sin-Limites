using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Audit;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Audit;

public class LogConfig : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("Logs", "dbo");
     builder.HasKey(l => l.LogId);
        builder.Property(l => l.FechaEventoUtc).HasColumnType("datetime2(0)");
      builder.Property(l => l.Operacion).IsRequired().HasMaxLength(20);
     builder.Property(l => l.Tabla).IsRequired().HasMaxLength(128);
        builder.Property(l => l.ClavePrimaria).IsRequired().HasMaxLength(500);
        builder.Property(l => l.Origen).HasMaxLength(80);
      builder.Property(l => l.Comentario).HasMaxLength(300);

     builder.HasOne(l => l.UsuarioActor).WithMany().HasForeignKey(l => l.UsuarioActorId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
