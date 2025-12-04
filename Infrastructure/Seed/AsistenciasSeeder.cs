using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder para Asistencias (genera datos para alertas del motor de inferencia)
/// </summary>
public static class AsistenciasSeeder
{
    private static readonly string?[] Observaciones = {
        "Participó activamente", "Llegó tarde", "Se retiró temprano",
        "Excelente participación", "Mostró interés", "Poca participación",
        null, null, null, null // 40% sin observación
    };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        Console.WriteLine("? Seeding Asistencias...");

        if (await context.Asistencias.AnyAsync())
        {
            Console.WriteLine("??  Asistencias ya existen, saltando...");
            return;
        }

        var inscripciones = await context.ActividadParticipantes
            .Include(ap => ap.Actividad)
            .Include(ap => ap.Participante)
            .Where(ap => ap.Actividad.Estado == EstadoActividad.Realizada && 
                        ap.Estado == EstadoInscripcion.Inscrito)
            .OrderBy(ap => ap.Actividad.FechaInicio)
            .ThenBy(ap => ap.ParticipanteId)
            .ToListAsync();

        if (!inscripciones.Any())
        {
            Console.WriteLine("?? No hay inscripciones válidas para generar asistencias.");
            return;
        }

        var random = new Random(42);
        var asistencias = new List<Asistencia>();

        // ========== ESCENARIOS PARA MOTOR DE INFERENCIA ==========

        // ESCENARIO 1: Participantes con BAJA ASISTENCIA GENERAL (< 75%) -> alertas ALTA
        var participantesConBajaAsistencia = inscripciones
            .GroupBy(i => i.ParticipanteId)
            .Take(5)
            .Select(g => g.Key)
            .ToHashSet();

        Console.WriteLine($"?? {participantesConBajaAsistencia.Count} participantes con baja asistencia (alertas ALTA)");

        // ESCENARIO 2: Participantes con INASISTENCIAS CONSECUTIVAS (3+) -> alertas ALTA
        var participantesConInasistenciasConsecutivas = inscripciones
            .GroupBy(i => i.ParticipanteId)
            .Skip(5)
            .Take(3)
            .Select(g => g.Key)
            .ToHashSet();

        Console.WriteLine($"?? {participantesConInasistenciasConsecutivas.Count} participantes con inasistencias consecutivas (alertas ALTA)");

        // Agrupar inscripciones por participante para el patrón de consecutivas
        var inscripcionesPorParticipante = inscripciones
            .GroupBy(i => i.ParticipanteId)
            .ToDictionary(g => g.Key, g => g.OrderBy(i => i.Actividad.FechaInicio).ToList());

        foreach (var inscripcion in inscripciones)
        {
            EstadoAsistencia estado;

            if (participantesConBajaAsistencia.Contains(inscripcion.ParticipanteId))
            {
                // Baja asistencia: ~40% presente, ~20% tarde, ~40% ausente
                var dado = random.Next(100);
                estado = dado < 40 ? EstadoAsistencia.Presente 
                       : dado < 60 ? EstadoAsistencia.Tarde 
                       : EstadoAsistencia.Ausente;
            }
            else if (participantesConInasistenciasConsecutivas.Contains(inscripcion.ParticipanteId))
            {
                // Patrón: P, P, A, A, A, A, P, P (ausentes en índices 2-5)
                var listaDelParticipante = inscripcionesPorParticipante[inscripcion.ParticipanteId];
                var indice = listaDelParticipante.IndexOf(inscripcion);
                
                estado = (indice >= 2 && indice <= 5) 
                    ? EstadoAsistencia.Ausente 
                    : EstadoAsistencia.Presente;
            }
            else
            {
                // Distribución normal: 82% presente, 10% ausente, 5% tarde, 3% justificado
                var dado = random.Next(100);
                estado = dado < 82 ? EstadoAsistencia.Presente
                       : dado < 92 ? EstadoAsistencia.Ausente
                       : dado < 97 ? EstadoAsistencia.Tarde
                       : EstadoAsistencia.Justificado;
            }

            asistencias.Add(new Asistencia
            {
                ActividadId = inscripcion.ActividadId,
                ParticipanteId = inscripcion.ParticipanteId,
                Fecha = inscripcion.Actividad.FechaInicio.Date,
                Estado = estado,
                Observacion = random.Next(100) < 30 ? Observaciones[random.Next(Observaciones.Length)] : null,
                CreadoEn = DateTime.UtcNow
            });
        }

        await context.Asistencias.AddRangeAsync(asistencias);
        await context.SaveChangesAsync();

        // Estadísticas
        var presentes = asistencias.Count(a => a.Estado == EstadoAsistencia.Presente);
        var ausentes = asistencias.Count(a => a.Estado == EstadoAsistencia.Ausente);
        var tardes = asistencias.Count(a => a.Estado == EstadoAsistencia.Tarde);
        var justificados = asistencias.Count(a => a.Estado == EstadoAsistencia.Justificado);

        Console.WriteLine($"? {asistencias.Count} Asistencias creadas");
        Console.WriteLine($"   - Presentes: {presentes} ({presentes * 100 / asistencias.Count}%)");
        Console.WriteLine($"   - Ausentes: {ausentes} ({ausentes * 100 / asistencias.Count}%)");
        Console.WriteLine($"   - Tardes: {tardes} ({tardes * 100 / asistencias.Count}%)");
        Console.WriteLine($"   - Justificados: {justificados} ({justificados * 100 / asistencias.Count}%)");
    }
}
