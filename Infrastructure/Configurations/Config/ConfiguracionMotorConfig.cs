using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Config;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Config;

public class ConfiguracionMotorConfig : IEntityTypeConfiguration<ConfiguracionMotor>
{
    public void Configure(EntityTypeBuilder<ConfiguracionMotor> builder)
    {
        builder.ToTable("ConfiguracionMotor", "dbo");
        builder.HasKey(c => c.Clave);
        builder.Property(c => c.Clave).IsRequired().HasMaxLength(80);
      builder.Property(c => c.Valor).IsRequired().HasMaxLength(400);
        builder.Property(c => c.Descripcion).HasMaxLength(300);
      builder.Property(c => c.RowVersion).IsRowVersion();

   builder.HasMany(c => c.Overrides).WithOne(o => o.ConfiguracionBase)
            .HasForeignKey(o => o.Clave).OnDelete(DeleteBehavior.NoAction);
    }
}
