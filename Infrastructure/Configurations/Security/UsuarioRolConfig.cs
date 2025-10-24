using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Security;

public class UsuarioRolConfig : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
      // La tabla ya está configurada en ApplicationDbContext
        // builder.ToTable("UsuarioRol", "dbo");

        // PK compuesta ya configurada por Identity (UserId, RoleId)

  builder.Property(ur => ur.RowVersion)
    .IsRowVersion();

        // Configurar campos de auditoría
 builder.Property(ur => ur.CreadoEn)
     .HasColumnType("datetime2(0)");
      
   builder.Property(ur => ur.ActualizadoEn)
      .HasColumnType("datetime2(0)");
        
builder.Property(ur => ur.EliminadoEn)
    .HasColumnType("datetime2(0)");

        builder.Property(ur => ur.AsignadoEn)
       .HasColumnType("datetime2(0)");

 // Las relaciones ya están definidas en UsuarioConfig y RolConfig
    }
}
