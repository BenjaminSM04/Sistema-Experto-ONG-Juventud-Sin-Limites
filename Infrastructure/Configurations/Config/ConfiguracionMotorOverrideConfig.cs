using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Config;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Config;

public class ConfiguracionMotorOverrideConfig : IEntityTypeConfiguration<ConfiguracionMotorOverride>
{
    public void Configure(EntityTypeBuilder<ConfiguracionMotorOverride> builder)
    {
   builder.ToTable("ConfiguracionMotorOverride", "dbo");
        builder.HasKey(o => new { o.ProgramaId, o.Clave });
     builder.Property(o => o.Clave).IsRequired().HasMaxLength(80);
        builder.Property(o => o.Valor).IsRequired().HasMaxLength(400);
        builder.Property(o => o.Descripcion).HasMaxLength(300);
        builder.Property(o => o.RowVersion).IsRowVersion();

        builder.HasIndex(o => o.ProgramaId).HasDatabaseName("IX_ConfigMotorOverride_Programa");

  builder.HasOne(o => o.Programa).WithMany().HasForeignKey(o => o.ProgramaId).OnDelete(DeleteBehavior.NoAction);
    }
}
