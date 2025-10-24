using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Security;

public class RolConfig : IEntityTypeConfiguration<Rol>
{
  public void Configure(EntityTypeBuilder<Rol> builder)
    {
        // La tabla ya est� configurada en ApplicationDbContext
        // builder.ToTable("Rol", "dbo");

    // Configuraciones adicionales
  builder.Property(r => r.Descripcion)
      .HasMaxLength(250);

        builder.Property(r => r.RowVersion)
   .IsRowVersion();

   // Configurar campos de auditor�a
        builder.Property(r => r.CreadoEn)
      .HasColumnType("datetime2(0)");
    
     builder.Property(r => r.ActualizadoEn)
      .HasColumnType("datetime2(0)");
        
   builder.Property(r => r.EliminadoEn)
 .HasColumnType("datetime2(0)");

   // �ndice �nico filtrado por Name (de IdentityRole)
builder.HasIndex(r => r.Name)
.IsUnique()
       .HasFilter("[IsDeleted] = 0")
     .HasDatabaseName("IX_Rol_Nombre_Unique");

 // Relaciones
        builder.HasMany(r => r.UsuarioRoles)
     .WithOne(ur => ur.Rol)
        .HasForeignKey(ur => ur.RoleId) // RoleId de IdentityUserRole
     .OnDelete(DeleteBehavior.Cascade);
    }
}
