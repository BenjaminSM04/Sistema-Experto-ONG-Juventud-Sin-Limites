using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference;
using Xunit;

namespace Sistema_Experto_ONG.Tests.Infrastructure.Services;

public class MotorInferenciaTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly MotorInferencia _motor;
    private readonly FeatureProvider _features;

    public MotorInferenciaTests()
    {
        // Configurar InMemory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
     .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _features = new FeatureProvider(_context);
        _motor = new MotorInferencia(_context, _features);

    // Seed datos de prueba
      SeedTestData();
    }

    private void SeedTestData()
    {
      // Programa de prueba
        var programa = new Programa
    {
    Clave = "TEST",
      Nombre = "Programa de Prueba",
      Estado = EstadoGeneral.Activo,
            InferenciaActiva = true,
      CreadoEn = DateTime.UtcNow
      };
        _context.Programas.Add(programa);
    _context.SaveChanges();

      // Regla INASISTENCIA_CONSECUTIVA
        var reglaInasistencia = new Regla
        {
         Clave = "INASISTENCIA_CONSECUTIVA",
            Nombre = "Inasistencia Consecutiva",
            Descripcion = "Test",
     Severidad = Severidad.Alta,
         Objetivo = ObjetivoRegla.Participante,
         Activa = true,
            Prioridad = 100,
      Version = 1,
     CreadoEn = DateTime.UtcNow
        };
        _context.Reglas.Add(reglaInasistencia);
        _context.SaveChanges();

        // Parámetro de regla
     _context.ReglaParametros.Add(new ReglaParametro
     {
         ReglaId = reglaInasistencia.ReglaId,
       Nombre = "UMBRAL_AUSENCIAS",
 Tipo = TipoParametro.Entero,
            Valor = "3",
          CreadoEn = DateTime.UtcNow
        });

   // Regla BAJA_ASISTENCIA_GENERAL
        var reglaBajaAsistencia = new Regla
        {
      Clave = "BAJA_ASISTENCIA_GENERAL",
            Nombre = "Baja Asistencia General",
            Descripcion = "Test",
            Severidad = Severidad.Alta,
            Objetivo = ObjetivoRegla.Participante,
       Activa = true,
            Prioridad = 90,
    Version = 1,
            CreadoEn = DateTime.UtcNow
   };
        _context.Reglas.Add(reglaBajaAsistencia);
        _context.SaveChanges();

        _context.ReglaParametros.Add(new ReglaParametro
     {
        ReglaId = reglaBajaAsistencia.ReglaId,
         Nombre = "UMBRAL_PCT",
     Tipo = TipoParametro.Decimal,
 Valor = "75",
            CreadoEn = DateTime.UtcNow
        });

 // Regla ACTIVIDAD_SIN_ASISTENTES
        var reglaSinAsistentes = new Regla
        {
    Clave = "ACTIVIDAD_SIN_ASISTENTES",
         Nombre = "Actividad sin asistentes",
       Descripcion = "Test",
        Severidad = Severidad.Info,
    Objetivo = ObjetivoRegla.Actividad,
            Activa = true,
      Prioridad = 50,
            Version = 1,
  CreadoEn = DateTime.UtcNow
   };
        _context.Reglas.Add(reglaSinAsistentes);
        _context.SaveChanges();

        _context.ReglaParametros.Add(new ReglaParametro
      {
    ReglaId = reglaSinAsistentes.ReglaId,
   Nombre = "MIN_INSCRITOS",
            Tipo = TipoParametro.Entero,
  Valor = "1",
            CreadoEn = DateTime.UtcNow
        });

        // Regla RETRASO_ACTIVIDAD
        var reglaRetraso = new Regla
  {
            Clave = "RETRASO_ACTIVIDAD",
     Nombre = "Retraso en actividad",
            Descripcion = "Test",
  Severidad = Severidad.Alta,
            Objetivo = ObjetivoRegla.Actividad,
    Activa = true,
    Prioridad = 80,
            Version = 1,
      CreadoEn = DateTime.UtcNow
        };
    _context.Reglas.Add(reglaRetraso);
  _context.SaveChanges();

        _context.ReglaParametros.Add(new ReglaParametro
        {
      ReglaId = reglaRetraso.ReglaId,
     Nombre = "DIAS_RETRASO",
Tipo = TipoParametro.Entero,
         Valor = "3",
  CreadoEn = DateTime.UtcNow
 });

      // Regla BAJO_CUMPLIMIENTO_POA
        var reglaBajoCumplimiento = new Regla
   {
    Clave = "BAJO_CUMPLIMIENTO_POA",
            Nombre = "Bajo Cumplimiento de POA",
       Descripcion = "Test",
     Severidad = Severidad.Critica,
   Objetivo = ObjetivoRegla.Programa,
    Activa = true,
     Prioridad = 100,
    Version = 1,
   CreadoEn = DateTime.UtcNow
        };
  _context.Reglas.Add(reglaBajoCumplimiento);
        _context.SaveChanges();

        _context.ReglaParametros.Add(new ReglaParametro
     {
            ReglaId = reglaBajoCumplimiento.ReglaId,
          Nombre = "UMBRAL_PCT",
     Tipo = TipoParametro.Decimal,
 Valor = "20",
          CreadoEn = DateTime.UtcNow
 });

     _context.SaveChanges();
    }

    [Fact]
    public async Task EjecutarMotor_DebeCrearEjecucionMotor()
  {
  // Arrange
        var fechaCorte = DateOnly.FromDateTime(DateTime.Now);

 // Act
    var resultado = await _motor.EjecutarAsync(fechaCorte, null, false, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.ReglasEjecutadas > 0);
        
     var ejecucion = await _context.EjecucionesMotor.FirstOrDefaultAsync();
    Assert.NotNull(ejecucion);
      Assert.NotNull(ejecucion.FinUtc);
    }

    [Fact]
    public async Task ReglaInasistenciaConsecutiva_ConDatosValidos_DebeGenerarAlerta()
    {
// Arrange
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

        var programa = await _context.Programas.FirstAsync();

        var actividad = new Actividad
        {
     ProgramaId = programa.ProgramaId,
       Titulo = "Actividad Test",
      FechaInicio = DateTime.Now.AddDays(-10),
        Tipo = TipoActividad.Taller,
     Estado = EstadoActividad.Planificada,
         CreadoEn = DateTime.UtcNow
     };
        _context.Actividades.Add(actividad);
   _context.SaveChanges();

        _context.ActividadParticipantes.Add(new ActividadParticipante
        {
            ActividadId = actividad.ActividadId,
          ParticipanteId = participante.ParticipanteId,
        Rol = RolParticipante.Asistente,
   Estado = EstadoInscripcion.Inscrito,
          CreadoEn = DateTime.UtcNow
     });
 _context.SaveChanges();

        // Crear 3 asistencias ausentes
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
        var alerta = await _context.Alertas.FirstOrDefaultAsync();
        Assert.NotNull(alerta);
        Assert.Equal(participante.ParticipanteId, alerta.ParticipanteId);
        Assert.Equal(EstadoAlerta.Abierta, alerta.Estado);
    }

    [Fact]
    public async Task ReglaBajaAsistencia_ConAsistenciaBaja_DebeGenerarAlerta()
{
        // Arrange
        var persona = new Persona
        {
    Nombres = "Test",
          Apellidos = "Baja Asistencia",
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

        var programa = await _context.Programas.FirstAsync();

     var actividad = new Actividad
        {
     ProgramaId = programa.ProgramaId,
            Titulo = "Actividad Test",
        FechaInicio = DateTime.Now.AddDays(-10),
            Tipo = TipoActividad.Taller,
         Estado = EstadoActividad.Planificada,
            CreadoEn = DateTime.UtcNow
        };
      _context.Actividades.Add(actividad);
        _context.SaveChanges();

     _context.ActividadParticipantes.Add(new ActividadParticipante
 {
         ActividadId = actividad.ActividadId,
            ParticipanteId = participante.ParticipanteId,
        Rol = RolParticipante.Asistente,
            Estado = EstadoInscripcion.Inscrito,
            CreadoEn = DateTime.UtcNow
        });
 _context.SaveChanges();

        // 1 presente, 2 ausentes = 33% (< 75%)
    _context.Asistencias.Add(new Asistencia
        {
    ActividadId = actividad.ActividadId,
    ParticipanteId = participante.ParticipanteId,
         Fecha = DateTime.Now.AddDays(-1),
    Estado = EstadoAsistencia.Presente,
            CreadoEn = DateTime.UtcNow
  });

        for (int i = 2; i <= 3; i++)
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
        var resultado = await _motor.EjecutarAsync(fechaCorte, programa.ProgramaId, false, CancellationToken.None);

   // Assert
        Assert.True(resultado.AlertasGeneradas > 0);
        var alerta = await _context.Alertas.FirstOrDefaultAsync(a => a.ParticipanteId == participante.ParticipanteId);
        Assert.NotNull(alerta);
    }

    [Fact]
    public async Task ReglaActividadSinAsistentes_SinInscripciones_DebeGenerarAlerta()
    {
        // Arrange
        var programa = await _context.Programas.FirstAsync();

        var actividad = new Actividad
        {
            ProgramaId = programa.ProgramaId,
   Titulo = "Actividad Sin Inscritos",
         FechaInicio = DateTime.Now.AddDays(5),
            Tipo = TipoActividad.Taller,
         Estado = EstadoActividad.Planificada,
       CreadoEn = DateTime.UtcNow
        };
        _context.Actividades.Add(actividad);
   _context.SaveChanges();

        var fechaCorte = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var resultado = await _motor.EjecutarAsync(fechaCorte, null, false, CancellationToken.None);

        // Assert
        var alerta = await _context.Alertas.FirstOrDefaultAsync(a => a.ActividadId == actividad.ActividadId);
        Assert.NotNull(alerta);
        Assert.Contains("inscritos", alerta.Mensaje, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReglaRetrasoActividad_ConActividadRetrasada_DebeGenerarAlerta()
    {
        // Arrange
        var programa = await _context.Programas.FirstAsync();

        var actividad = new Actividad
        {
  ProgramaId = programa.ProgramaId,
      Titulo = "Actividad Retrasada",
      FechaInicio = DateTime.Now.AddDays(-10), // 10 días atrás
       Tipo = TipoActividad.Taller,
            Estado = EstadoActividad.Planificada, // Sigue planificada
      CreadoEn = DateTime.UtcNow
        };
        _context.Actividades.Add(actividad);
        _context.SaveChanges();

      var fechaCorte = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var resultado = await _motor.EjecutarAsync(fechaCorte, null, false, CancellationToken.None);

        // Assert
        var alerta = await _context.Alertas.FirstOrDefaultAsync(a => a.ActividadId == actividad.ActividadId);
        Assert.NotNull(alerta);
        Assert.Contains("retraso", alerta.Mensaje, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReglaBajoCumplimientoPOA_ConBajoCumplimiento_DebeGenerarAlerta()
    {
        // Arrange
        var programa = await _context.Programas.FirstAsync();
        var anioMes = DateTime.Now.ToString("yyyy-MM");

        var metricas = new Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.BI.MetricasProgramaMes
 {
     ProgramaId = programa.ProgramaId,
        AnioMes = anioMes,
   ActividadesPlanificadas = 10,
 ActividadesEjecutadas = 4, // 40% ejecutado = 60% gap
            PorcCumplimiento = 40m,
    RetrasoPromedioDias = 5m,
          PorcAsistenciaProm = 70m,
            CreadoEn = DateTime.UtcNow
      };
        _context.MetricasProgramaMes.Add(metricas);
        _context.SaveChanges();

        var fechaCorte = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var resultado = await _motor.EjecutarAsync(fechaCorte, programa.ProgramaId, false, CancellationToken.None);

        // Assert
      var alerta = await _context.Alertas.FirstOrDefaultAsync(a => a.ProgramaId == programa.ProgramaId);
        Assert.NotNull(alerta);
        Assert.Contains("Desvío", alerta.Mensaje);
    }

    [Fact]
    public async Task EjecutarMotor_ConReglasInactivas_NoDebeEjecutarlas()
    {
    // Arrange
 var reglas = await _context.Reglas.ToListAsync();
   foreach (var regla in reglas)
        {
  regla.Activa = false;
        }
        _context.SaveChanges();

        var fechaCorte = DateOnly.FromDateTime(DateTime.Now);

      // Act
        var resultado = await _motor.EjecutarAsync(fechaCorte, null, false, CancellationToken.None);

    // Assert
        Assert.Equal(0, resultado.AlertasGeneradas);
  }

  public void Dispose()
    {
_context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
