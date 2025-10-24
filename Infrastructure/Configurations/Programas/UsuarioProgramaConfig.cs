using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Programas;

public class UsuarioProgramaConfig : IEntityTypeConfiguration<UsuarioPrograma>
{
  public void Configure(EntityTypeBuilder<UsuarioPrograma> builder)
    {
        builder.ToTable("UsuarioPrograma", "dbo");

        // PK compuesta
        builder.HasKey(up => new { up.UsuarioId, up.ProgramaId, up.Desde });

        builder.Property(up => up.Desde)
     .HasColumnType("date");

      builder.Property(up => up.Hasta)
            .HasColumnType("date");

   builder.Property(up => up.RowVersion)
            .IsRowVersion();

        // Índices
        builder.HasIndex(up => up.UsuarioId)
            .HasFilter("[IsDeleted] = 0")
     .HasDatabaseName("IX_UsuarioPrograma_Usuario");

        builder.HasIndex(up => up.ProgramaId)
  .HasFilter("[IsDeleted] = 0")
        .HasDatabaseName("IX_UsuarioPrograma_Programa");
    }
}
