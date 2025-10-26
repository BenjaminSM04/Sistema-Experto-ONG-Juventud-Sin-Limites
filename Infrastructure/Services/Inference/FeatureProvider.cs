using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference;

/// <summary>
/// Proveedor de datos/características para el motor de inferencia
/// </summary>
public class FeatureProvider : IFeatureProvider
{
    private readonly ApplicationDbContext _context;

    public FeatureProvider(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<double> PorcAsistenciaParticipanteAsync(int participanteId, int programaId, DateOnly desde, DateOnly hasta, CancellationToken ct)
{
   // Convertir DateOnly a DateTime para comparación
   var desdeDateTime = desde.ToDateTime(TimeOnly.MinValue);
  var hastaDateTime = hasta.ToDateTime(TimeOnly.MaxValue);

        var asistencias = await _context.Asistencias
   .Include(a => a.Actividad)
            .Where(a => a.ParticipanteId == participanteId
         && a.Actividad.ProgramaId == programaId
 && a.Fecha >= desdeDateTime
     && a.Fecha <= hastaDateTime
         && !a.IsDeleted)
       .ToListAsync(ct);

  if (!asistencias.Any())
    return 100.0; // Si no hay asistencias registradas, asumimos 100%

        var presentes = asistencias.Count(a => a.Estado == EstadoAsistencia.Presente);
        var total = asistencias.Count;

  return (double)presentes / total * 100.0;
    }

    public async Task<int> ConsecutivasAusenciasAsync(int participanteId, int actividadId, DateOnly hasta, CancellationToken ct)
    {
        // Convertir DateOnly a DateTime para comparación
        var hastaDateTime = hasta.ToDateTime(TimeOnly.MaxValue);

 var asistencias = await _context.Asistencias
     .Where(a => a.ParticipanteId == participanteId
     && a.ActividadId == actividadId
        && a.Fecha <= hastaDateTime
      && !a.IsDeleted)
            .OrderByDescending(a => a.Fecha)
     .ToListAsync(ct);

      int consecutivas = 0;
        foreach (var asistencia in asistencias)
  {
       if (asistencia.Estado == EstadoAsistencia.Ausente)
  {
       consecutivas++;
            }
  else
   {
         break; // Si encuentra una asistencia presente, termina
  }
    }

  return consecutivas;
    }

    public async Task<decimal?> PoaDecimalAsync(int instanciaId, string campoClave, int? programaId, int? actividadId, int? participanteId, CancellationToken ct)
{
        var valor = await _context.POAValores
          .Where(v => v.InstanciaId == instanciaId
       && v.Campo.Clave == campoClave
       && !v.IsDeleted)
     .FirstOrDefaultAsync(ct);

        return valor?.ValorNumero;
}

    public async Task<(int plan, int ejec)> PlanVsEjecAsync(int programaId, string anioMes, CancellationToken ct)
  {
     var metricas = await _context.MetricasProgramaMes
            .Where(m => m.ProgramaId == programaId
         && m.AnioMes == anioMes
             && !m.IsDeleted)
            .FirstOrDefaultAsync(ct);

if (metricas == null)
   return (0, 0);

        return (metricas.ActividadesPlanificadas, metricas.ActividadesEjecutadas);
    }
}
