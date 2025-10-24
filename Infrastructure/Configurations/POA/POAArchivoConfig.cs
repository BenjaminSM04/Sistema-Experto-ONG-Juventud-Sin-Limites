using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.POA;

public class POAArchivoConfig : IEntityTypeConfiguration<POAArchivo>
{
    public void Configure(EntityTypeBuilder<POAArchivo> builder)
    {
     builder.ToTable("POA_Archivo", "dbo");

     builder.HasKey(a => a.ArchivoId);

   builder.Property(a => a.ArchivoPath)
  .IsRequired()
   .HasMaxLength(300);

   builder.Property(a => a.SubidoEn)
  .HasColumnType("datetime2(0)");

        builder.Property(a => a.RowVersion)
 .IsRowVersion();

 // Relación opcional con Campo
        builder.HasOne(a => a.Campo)
            .WithMany()
     .HasForeignKey(a => a.CampoId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
