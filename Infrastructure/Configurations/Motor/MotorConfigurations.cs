using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Configurations.Motor;

public class ReglaParametroConfig : IEntityTypeConfiguration<ReglaParametro>
{
    public void Configure(EntityTypeBuilder<ReglaParametro> builder)
    {
 builder.ToTable("ReglaParametro", "dbo");
        builder.HasKey(rp => rp.ReglaParametroId);
   builder.Property(rp => rp.Nombre).IsRequired().HasMaxLength(80);
        builder.Property(rp => rp.Tipo).HasConversion<byte>();
        builder.Property(rp => rp.Valor).IsRequired().HasMaxLength(400);
        builder.Property(rp => rp.RowVersion).IsRowVersion();
      
        builder.HasIndex(rp => new { rp.ReglaId, rp.Nombre })
            .IsUnique().HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_ReglaParam_Regla_Nombre");
    }
}

public class ReglaParametroOverrideConfig : IEntityTypeConfiguration<ReglaParametroOverride>
{
    public void Configure(EntityTypeBuilder<ReglaParametroOverride> builder)
    {
        builder.ToTable("ReglaParametroOverride", "dbo");
        builder.HasKey(rpo => new { rpo.ReglaId, rpo.ProgramaId, rpo.Nombre });
 builder.Property(rpo => rpo.Nombre).IsRequired().HasMaxLength(80);
    builder.Property(rpo => rpo.Tipo).HasConversion<byte>();
        builder.Property(rpo => rpo.Valor).IsRequired().HasMaxLength(400);
        builder.Property(rpo => rpo.RowVersion).IsRowVersion();
    }
}

public class AlertaConfig : IEntityTypeConfiguration<Alerta>
{
public void Configure(EntityTypeBuilder<Alerta> builder)
    {
        builder.ToTable("Alerta", "dbo");
        builder.HasKey(a => a.AlertaId);
        builder.Property(a => a.Severidad).HasConversion<byte>();
 builder.Property(a => a.Mensaje).IsRequired().HasMaxLength(300);
        builder.Property(a => a.Estado).HasConversion<byte>();
    builder.Property(a => a.GeneradaEn).HasColumnType("datetime2(0)");
  builder.Property(a => a.ResueltaEn).HasColumnType("datetime2(0)");
     builder.Property(a => a.Notas).HasMaxLength(500);
        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.HasIndex(a => new { a.ProgramaId, a.ActividadId, a.ParticipanteId })
            .HasFilter("[IsDeleted] = 0").HasDatabaseName("IX_Alerta_Targets");
        builder.HasIndex(a => new { a.Estado, a.Severidad })
.HasFilter("[IsDeleted] = 0").HasDatabaseName("IX_Alerta_Estado_Severidad");

        builder.HasOne(a => a.Programa).WithMany().HasForeignKey(a => a.ProgramaId).OnDelete(DeleteBehavior.NoAction);
     builder.HasOne(a => a.Actividad).WithMany().HasForeignKey(a => a.ActividadId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(a => a.Participante).WithMany().HasForeignKey(a => a.ParticipanteId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class RiesgoParticipanteProgramaConfig : IEntityTypeConfiguration<RiesgoParticipantePrograma>
{
    public void Configure(EntityTypeBuilder<RiesgoParticipantePrograma> builder)
    {
        builder.ToTable("RiesgoParticipantePrograma", "dbo");
        builder.HasKey(r => r.RiesgoId);
      builder.Property(r => r.FechaCorte).HasColumnType("date");
  builder.Property(r => r.Banda).HasConversion<byte>();
        builder.Property(r => r.ExplicacionCorta).HasMaxLength(300);
   builder.Property(r => r.RowVersion).IsRowVersion();

        builder.HasIndex(r => new { r.ParticipanteId, r.ProgramaId, r.FechaCorte })
  .HasFilter("[IsDeleted] = 0").HasDatabaseName("IX_Riesgo_Part_Prog_Fecha");

        builder.HasMany(r => r.Detalles).WithOne(d => d.Riesgo).HasForeignKey(d => d.RiesgoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RiesgoDetalleConfig : IEntityTypeConfiguration<RiesgoDetalle>
{
    public void Configure(EntityTypeBuilder<RiesgoDetalle> builder)
    {
        builder.ToTable("RiesgoDetalle", "dbo");
        builder.HasKey(rd => rd.RiesgoDetalleId);
        builder.Property(rd => rd.NombreFeature).IsRequired().HasMaxLength(120);
     builder.Property(rd => rd.ValorNumerico).HasPrecision(18, 6);
        builder.Property(rd => rd.ValorTexto).HasMaxLength(300);
        builder.Property(rd => rd.PesoContribucion).HasPrecision(9, 6);
        builder.Property(rd => rd.RowVersion).IsRowVersion();
    }
}

public class EjecucionMotorConfig : IEntityTypeConfiguration<EjecucionMotor>
{
    public void Configure(EntityTypeBuilder<EjecucionMotor> builder)
    {
     builder.ToTable("EjecucionMotor", "dbo");
  builder.HasKey(e => e.EjecucionId);
        builder.Property(e => e.InicioUtc).HasColumnType("datetime2(0)");
  builder.Property(e => e.FinUtc).HasColumnType("datetime2(0)");
        builder.Property(e => e.Ambito).HasConversion<byte>();
        builder.Property(e => e.ResultadoResumen).HasMaxLength(1000);
 builder.Property(e => e.RowVersion).IsRowVersion();

        builder.HasMany(e => e.Matches).WithOne(m => m.Ejecucion).HasForeignKey(m => m.EjecucionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MatchReglaConfig : IEntityTypeConfiguration<MatchRegla>
{
    public void Configure(EntityTypeBuilder<MatchRegla> builder)
    {
     builder.ToTable("MatchRegla", "dbo");
        builder.HasKey(m => m.MatchId);
        builder.Property(m => m.Mensaje).HasMaxLength(300);
        builder.Property(m => m.RowVersion).IsRowVersion();

   builder.HasOne(m => m.Programa).WithMany().HasForeignKey(m => m.ProgramaId).OnDelete(DeleteBehavior.NoAction);
      builder.HasOne(m => m.Actividad).WithMany().HasForeignKey(m => m.ActividadId).OnDelete(DeleteBehavior.NoAction);
     builder.HasOne(m => m.Participante).WithMany().HasForeignKey(m => m.ParticipanteId).OnDelete(DeleteBehavior.NoAction);
  }
}

public class DiccionarioObservacionesConfig : IEntityTypeConfiguration<DiccionarioObservaciones>
{
    public void Configure(EntityTypeBuilder<DiccionarioObservaciones> builder)
    {
        builder.ToTable("DiccionarioObservaciones", "dbo");
        builder.HasKey(d => d.DiccionarioId);
        builder.Property(d => d.Expresion).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Ponderacion).HasPrecision(9, 6);
  builder.Property(d => d.Ambito).HasConversion<byte>();
  builder.Property(d => d.RowVersion).IsRowVersion();

        builder.HasMany(d => d.DiccionarioProgramas).WithOne(dp => dp.Diccionario)
            .HasForeignKey(dp => dp.DiccionarioId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DiccionarioObservacionesProgramaConfig : IEntityTypeConfiguration<DiccionarioObservacionesPrograma>
{
    public void Configure(EntityTypeBuilder<DiccionarioObservacionesPrograma> builder)
    {
        builder.ToTable("DiccionarioObservacionesPrograma", "dbo");
        builder.HasKey(dp => new { dp.DiccionarioId, dp.ProgramaId });
  builder.Property(dp => dp.RowVersion).IsRowVersion();

     builder.HasOne(dp => dp.Programa).WithMany().HasForeignKey(dp => dp.ProgramaId).OnDelete(DeleteBehavior.NoAction);
    }
}
