using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Security;

public class PersonaConfig : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
 {
        builder.ToTable("Persona", "dbo");

     builder.HasKey(p => p.PersonaId);

        builder.Property(p => p.Nombres)
 .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Apellidos)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Telefono)
            .HasMaxLength(30);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Relaciones
        builder.HasOne(p => p.Usuario)
      .WithOne(u => u.Persona)
            .HasForeignKey<Usuario>(u => u.PersonaId)
        .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.Participante)
  .WithOne(pt => pt.Persona)
            .HasForeignKey<Domain.Operacion.Participante>(pt => pt.PersonaId)
     .OnDelete(DeleteBehavior.NoAction);
    }
}
