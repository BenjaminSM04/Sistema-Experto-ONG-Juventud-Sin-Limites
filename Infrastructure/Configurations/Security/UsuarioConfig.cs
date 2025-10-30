using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Security;

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
   // La tabla ya está configurada en ApplicationDbContext
        // builder.ToTable("Usuario", "dbo"); 

        // Clave primaria ya configurada por Identity (Id)
   // Agregar configuraciones adicionales

        builder.Property(u => u.Estado)
       .HasConversion<byte>();

  builder.Property(u => u.RowVersion)
    .IsRowVersion();

    // Configurar campos de gestión de contraseñas
 builder.Property(u => u.MustChangePassword)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(256);

    builder.Property(u => u.CreatedAtUtc)
    .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
      .HasColumnType("datetime2(0)");

    // Configurar campos de auditoría
  builder.Property(u => u.CreadoEn)
   .HasColumnType("datetime2(0)");
        
  builder.Property(u => u.ActualizadoEn)
   .HasColumnType("datetime2(0)");
        
   builder.Property(u => u.EliminadoEn)
   .HasColumnType("datetime2(0)");

        // Índice único filtrado por Email y IsDeleted
  builder.HasIndex(u => u.Email)
   .IsUnique()
        .HasFilter("[IsDeleted] = 0")
     .HasDatabaseName("IX_Usuario_Email_Unique");

        // Relación 1:1 con Persona
  builder.HasOne(u => u.Persona)
 .WithOne(p => p.Usuario!)
     .HasForeignKey<Usuario>(u => u.PersonaId)
     .OnDelete(DeleteBehavior.NoAction);

// Relaciones con UsuarioRol
        builder.HasMany(u => u.UsuarioRoles)
            .WithOne(ur => ur.Usuario)
   .HasForeignKey(ur => ur.UserId) // UserId de IdentityUserRole
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UsuarioProgramas)
      .WithOne(up => up.Usuario)
       .HasForeignKey(up => up.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
