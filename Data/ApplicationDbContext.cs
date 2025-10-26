using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Audit;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.BI;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Config;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Data
{
    /// <summary>
    /// DbContext que integra ASP.NET Core Identity con el modelo de dominio
    /// Usa Usuario, Rol, UsuarioRol personalizados que heredan de Identity
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, int,
        Microsoft.AspNetCore.Identity.IdentityUserClaim<int>,
        UsuarioRol,
 Microsoft.AspNetCore.Identity.IdentityUserLogin<int>,
        Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>,
        Microsoft.AspNetCore.Identity.IdentityUserToken<int>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Security (ahora integrado con Identity)
        public DbSet<Persona> Personas => Set<Persona>();
        // Usuario, Rol, UsuarioRol ya están en IdentityDbContext como Users, Roles, UserRoles

        // Programas
        public DbSet<Programa> Programas => Set<Programa>();
        public DbSet<UsuarioPrograma> UsuarioProgramas => Set<UsuarioPrograma>();

        // Operacion
        public DbSet<Participante> Participantes => Set<Participante>();
        public DbSet<Actividad> Actividades => Set<Actividad>();
        public DbSet<ActividadParticipante> ActividadParticipantes => Set<ActividadParticipante>();
        public DbSet<Asistencia> Asistencias => Set<Asistencia>();
        public DbSet<EvidenciaActividad> EvidenciaActividades => Set<EvidenciaActividad>();

        // POA
        public DbSet<POAPlantilla> POAPlantillas => Set<POAPlantilla>();
        public DbSet<POAPlantillaSeccion> POAPlantillaSecciones => Set<POAPlantillaSeccion>();
        public DbSet<POACampo> POACampos => Set<POACampo>();
        public DbSet<POACampoOpcion> POACampoOpciones => Set<POACampoOpcion>();
        public DbSet<POACampoValidacion> POACampoValidaciones => Set<POACampoValidacion>();
        public DbSet<POAInstancia> POAInstancias => Set<POAInstancia>();
        public DbSet<POAValor> POAValores => Set<POAValor>();
        public DbSet<POAArchivo> POAArchivos => Set<POAArchivo>();
        public DbSet<POASnapshotMensual> POASnapshotMensuales => Set<POASnapshotMensual>();

        // Motor
        public DbSet<Regla> Reglas => Set<Regla>();
        public DbSet<ReglaParametro> ReglaParametros => Set<ReglaParametro>();
        public DbSet<ReglaParametroOverride> ReglaParametroOverrides => Set<ReglaParametroOverride>();
        public DbSet<Alerta> Alertas => Set<Alerta>();
        public DbSet<RiesgoParticipantePrograma> RiesgosParticipantePrograma => Set<RiesgoParticipantePrograma>();
        public DbSet<RiesgoDetalle> RiesgoDetalles => Set<RiesgoDetalle>();
        public DbSet<EjecucionMotor> EjecucionesMotor => Set<EjecucionMotor>();
        public DbSet<MatchRegla> MatchReglas => Set<MatchRegla>();
        public DbSet<DiccionarioObservaciones> DiccionarioObservaciones => Set<DiccionarioObservaciones>();
        public DbSet<DiccionarioObservacionesPrograma> DiccionarioObservacionesProgramas => Set<DiccionarioObservacionesPrograma>();

        // Config
        public DbSet<ConfiguracionMotor> ConfiguracionesMotor => Set<ConfiguracionMotor>();
        public DbSet<ConfiguracionMotorOverride> ConfiguracionMotorOverrides => Set<ConfiguracionMotorOverride>();

        // BI
        public DbSet<MetricasProgramaMes> MetricasProgramaMes => Set<MetricasProgramaMes>();

        // Audit
        public DbSet<Log> Logs => Set<Log>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Renombrar tablas de Identity para que coincidan con tu esquema
            modelBuilder.Entity<Usuario>().ToTable("Usuario", "dbo");
            modelBuilder.Entity<Rol>().ToTable("Rol", "dbo");
            modelBuilder.Entity<UsuarioRol>().ToTable("UsuarioRol", "dbo");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<int>>().ToTable("UsuarioClaim", "dbo");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<int>>().ToTable("UsuarioLogin", "dbo");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>>().ToTable("RolClaim", "dbo");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<int>>().ToTable("UsuarioToken", "dbo");

            // Aplicar todas las configuraciones Fluent API
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Query Filter global para SoftDelete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(Domain.Common.ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var property = System.Linq.Expressions.Expression.Property(parameter, nameof(Domain.Common.ISoftDelete.IsDeleted));
                    var filterExpression = System.Linq.Expressions.Expression.Lambda(
                             System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                      parameter
                        );
                    entityType.SetQueryFilter(filterExpression);
                }
            }
        }
    }
}
