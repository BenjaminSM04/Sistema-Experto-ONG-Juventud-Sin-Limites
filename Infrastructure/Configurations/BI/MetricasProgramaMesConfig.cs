using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.BI;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.BI;

public class MetricasProgramaMesConfig : IEntityTypeConfiguration<MetricasProgramaMes>
{
    public void Configure(EntityTypeBuilder<MetricasProgramaMes> builder)
 {
        builder.ToTable("MetricasProgramaMes", "dbo");
        builder.HasKey(m => new { m.ProgramaId, m.AnioMes });
        builder.Property(m => m.AnioMes).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(m => m.PorcCumplimiento).HasPrecision(5, 2);
        builder.Property(m => m.RetrasoPromedioDias).HasPrecision(9, 2);
        builder.Property(m => m.PorcAsistenciaProm).HasPrecision(5, 2);
   builder.Property(m => m.RowVersion).IsRowVersion();

    builder.HasOne(m => m.Programa).WithMany().HasForeignKey(m => m.ProgramaId).OnDelete(DeleteBehavior.NoAction);
}
}
