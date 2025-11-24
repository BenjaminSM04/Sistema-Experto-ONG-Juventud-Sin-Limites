using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference;
using Xunit;

namespace Sistema_Experto_ONG.Tests.Infrastructure.Services;

public class FeatureProviderTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FeatureProvider _provider;

    public FeatureProviderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
         .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
 _provider = new FeatureProvider(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
   // Programa
        var programa = new Programa
{
       Clave = "TEST",
   Nombre = "Programa Test",
            Estado = EstadoGeneral.Activo,
InferenciaActiva = true,
            CreadoEn = DateTime.UtcNow
        };
        _context.Programas.Add(programa);
        _context.SaveChanges();

        // Persona y Participante
        var persona = new Persona
    {
      Nombres = "Test",
       Apellidos = "Usuario",
            FechaNacimiento = new DateTime(2005, 1, 1),
        CreadoEn = DateTime.UtcNow
        };
        _context.Personas.Add(persona);
  _context.SaveChanges();

        var participante = new Participante
        {
PersonaId = persona.PersonaId,
          Estado = EstadoGeneral.Activo,
   FechaAlta = DateTime.Now,
CreadoEn = DateTime.UtcNow
        };
        _context.Participantes.Add(participante);
        _context.SaveChanges();

        // Actividad
        var actividad = new Actividad
        {
 ProgramaId = programa.ProgramaId,
    Titulo = "Actividad Test",
     FechaInicio = DateTime.Now.AddDays(-30),
        Tipo = TipoActividad.Taller,
     Estado = EstadoActividad.Planificada,
   CreadoEn = DateTime.UtcNow
        };
      _context.Actividades.Add(actividad);
   _context.SaveChanges();

     // Inscripción
        _context.ActividadParticipantes.Add(new ActividadParticipante
        {
        ActividadId = actividad.ActividadId,
 ParticipanteId = participante.ParticipanteId,
      Rol = RolParticipante.Asistente,
          Estado = EstadoInscripcion.Inscrito,
     CreadoEn = DateTime.UtcNow
        });
_context.SaveChanges();
    }

    [Fact]
    public async Task ConsecutivasAusenciasAsync_ConAusenciasConsecutivas_DebeContarCorrectamente()
  {
        // Arrange
        var participante = await _context.Participantes.FirstAsync();
     var actividad = await _context.Actividades.FirstAsync();

      // 3 ausencias consecutivas
        for (int i = 1; i <= 3; i++)
        {
      _context.Asistencias.Add(new Asistencia
  {
       ActividadId = actividad.ActividadId,
   ParticipanteId = participante.ParticipanteId,
           Fecha = DateTime.Now.AddDays(-i),
                Estado = EstadoAsistencia.Ausente,
          CreadoEn = DateTime.UtcNow
      });
        }
        _context.SaveChanges();

 var fechaCorte = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var resultado = await _provider.ConsecutivasAusenciasAsync(
            participante.ParticipanteId,
     actividad.ActividadId,
    fechaCorte,
    CancellationToken.None);

      // Assert
  Assert.Equal(3, resultado);
    }

    [Fact]
    public async Task ConsecutivasAusenciasAsync_ConAsistenciaEnMedio_DebeReiniciarConteo()
    {
        // Arrange
        var participante = await _context.Participantes.FirstAsync();
   var actividad = await _context.Actividades.FirstAsync();

        // Patrón: Ausente, Ausente, Presente, Ausente
        _context.Asistencias.Add(new Asistencia
        {
     ActividadId = actividad.ActividadId,
            ParticipanteId = participante.ParticipanteId,
    Fecha = DateTime.Now.AddDays(-4),
            Estado = EstadoAsistencia.Ausente,
      CreadoEn = DateTime.UtcNow
     });

        _context.Asistencias.Add(new Asistencia
        {
     ActividadId = actividad.ActividadId,
            ParticipanteId = participante.ParticipanteId,
            Fecha = DateTime.Now.AddDays(-3),
   Estado = EstadoAsistencia.Ausente,
     CreadoEn = DateTime.UtcNow
      });

        _context.Asistencias.Add(new Asistencia
        {
      ActividadId = actividad.ActividadId,
   ParticipanteId = participante.ParticipanteId,
  Fecha = DateTime.Now.AddDays(-2),
   Estado = EstadoAsistencia.Presente, // Rompe la racha
            CreadoEn = DateTime.UtcNow
        });

        _context.Asistencias.Add(new Asistencia
        {
      ActividadId = actividad.ActividadId,
            ParticipanteId = participante.ParticipanteId,
Fecha = DateTime.Now.AddDays(-1),
         Estado = EstadoAsistencia.Ausente,
            CreadoEn = DateTime.UtcNow
        });

     _context.SaveChanges();

        var fechaCorte = DateOnly.FromDateTime(DateTime.Now);

        // Act
   var resultado = await _provider.ConsecutivasAusenciasAsync(
    participante.ParticipanteId,
 actividad.ActividadId,
            fechaCorte,
       CancellationToken.None);

      // Assert
     Assert.Equal(1, resultado); // Solo la última ausencia
    }

    [Fact]
    public async Task PorcAsistenciaParticipanteAsync_ConAsistenciasMixtas_DebeCalcularCorrectamente()
    {
      // Arrange
     var participante = await _context.Participantes.FirstAsync();
    var programa = await _context.Programas.FirstAsync();
        var actividad = await _context.Actividades.FirstAsync();

 // 7 presentes, 3 ausentes = 70%
for (int i = 1; i <= 7; i++)
        {
    _context.Asistencias.Add(new Asistencia
            {
       ActividadId = actividad.ActividadId,
         ParticipanteId = participante.ParticipanteId,
       Fecha = DateTime.Now.AddDays(-i),
                Estado = EstadoAsistencia.Presente,
        CreadoEn = DateTime.UtcNow
      });
        }

        for (int i = 8; i <= 10; i++)
        {
            _context.Asistencias.Add(new Asistencia
    {
        ActividadId = actividad.ActividadId,
     ParticipanteId = participante.ParticipanteId,
     Fecha = DateTime.Now.AddDays(-i),
                Estado = EstadoAsistencia.Ausente,
              CreadoEn = DateTime.UtcNow
            });
   }
        _context.SaveChanges();

        var desde = DateOnly.FromDateTime(DateTime.Now.AddDays(-15));
        var hasta = DateOnly.FromDateTime(DateTime.Now);

 // Act
        var resultado = await _provider.PorcAsistenciaParticipanteAsync(
      participante.ParticipanteId,
          programa.ProgramaId,
     desde,
        hasta,
            CancellationToken.None);

        // Assert
        Assert.Equal(70.0, resultado, 1); // 70% con 1 decimal de tolerancia
    }

    [Fact]
    public async Task PorcAsistenciaParticipanteAsync_SinAsistencias_DebeRetornarCero()
    {
        // Arrange
        var participante = await _context.Participantes.FirstAsync();
        var programa = await _context.Programas.FirstAsync();

        var desde = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        var hasta = DateOnly.FromDateTime(DateTime.Now);

      // Act
        var resultado = await _provider.PorcAsistenciaParticipanteAsync(
            participante.ParticipanteId,
            programa.ProgramaId,
            desde,
            hasta,
            CancellationToken.None);

        // Assert
        Assert.Equal(0.0, resultado);
    }

    [Fact]
    public async Task PlanVsEjecAsync_ConMetricas_DebeRetornarValoresCorrectos()
    {
        // Arrange
   var programa = await _context.Programas.FirstAsync();
        var anioMes = DateTime.Now.ToString("yyyy-MM");

        var metricas = new Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.BI.MetricasProgramaMes
        {
     ProgramaId = programa.ProgramaId,
            AnioMes = anioMes,
            ActividadesPlanificadas = 10,
            ActividadesEjecutadas = 7,
            PorcCumplimiento = 70m,
 RetrasoPromedioDias = 2m,
            PorcAsistenciaProm = 85m,
     CreadoEn = DateTime.UtcNow
   };
        _context.MetricasProgramaMes.Add(metricas);
        _context.SaveChanges();

        // Act
  var (plan, ejec) = await _provider.PlanVsEjecAsync(
     programa.ProgramaId,
            anioMes,
            CancellationToken.None);

        // Assert
        Assert.Equal(10m, plan);
        Assert.Equal(7m, ejec);
    }

    [Fact]
    public async Task PlanVsEjecAsync_SinMetricas_DebeRetornarCero()
    {
        // Arrange
    var programa = await _context.Programas.FirstAsync();
        var anioMes = "2099-12"; // Mes que no existe

        // Act
        var (plan, ejec) = await _provider.PlanVsEjecAsync(
            programa.ProgramaId,
 anioMes,
            CancellationToken.None);

     // Assert
        Assert.Equal(0m, plan);
        Assert.Equal(0m, ejec);
    }

    /*
    [Fact]
    public async Task PoaDecimalAsync_ConPOA_DebeRetornarValorCorrecto()
    {
        // ... (commented out code)
    }
    */

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
