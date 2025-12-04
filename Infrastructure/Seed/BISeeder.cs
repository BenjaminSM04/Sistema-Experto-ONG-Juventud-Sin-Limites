using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.BI;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder para Evidencias y Métricas de BI
/// </summary>
public static class BISeeder
{
    public static async Task SeedEvidenciasAsync(ApplicationDbContext context)
    {
        Console.WriteLine("?? Seeding Evidencias de Actividades...");

        if (await context.EvidenciaActividades.AnyAsync())
        {
            Console.WriteLine("??  Evidencias ya existen, saltando...");
            return;
        }

        var actividades = await context.Actividades
            .Where(a => a.Estado == EstadoActividad.Realizada && !a.IsDeleted)
            .ToListAsync();

        if (!actividades.Any())
        {
            Console.WriteLine("?? No hay actividades realizadas.");
            return;
        }

        var random = new Random(42);
        var evidencias = new List<EvidenciaActividad>();

        var tiposEvidencia = new[] {
            TipoEvidencia.Foto,
            TipoEvidencia.Video,
            TipoEvidencia.Acta,
            TipoEvidencia.Lista
        };

        foreach (var actividad in actividades)
        {
            // 70% de actividades tienen evidencias
            if (random.Next(100) < 70)
            {
                var cantidadEvidencias = random.Next(1, 4);

                for (int i = 0; i < cantidadEvidencias; i++)
                {
                    var tipo = tiposEvidencia[random.Next(tiposEvidencia.Length)];
                    var extension = tipo switch
                    {
                        TipoEvidencia.Foto => ".jpg",
                        TipoEvidencia.Video => ".mp4",
                        TipoEvidencia.Acta => ".pdf",
                        TipoEvidencia.Lista => ".xlsx",
                        _ => ".jpg"
                    };

                    evidencias.Add(new EvidenciaActividad
                    {
                        ActividadId = actividad.ActividadId,
                        Tipo = tipo,
                        ArchivoPath = $"/evidencias/{actividad.ActividadId}/{tipo}_{i + 1}{extension}",
                        SubidoEn = actividad.FechaInicio.AddDays(random.Next(1, 8)),
                        CreadoEn = DateTime.UtcNow
                    });
                }
            }
        }

        await context.EvidenciaActividades.AddRangeAsync(evidencias);
        await context.SaveChangesAsync();

        Console.WriteLine($"? {evidencias.Count} Evidencias creadas para {actividades.Count} actividades");
    }

    public static async Task SeedMetricasAsync(ApplicationDbContext context)
    {
        Console.WriteLine("?? Seeding Métricas Mensuales...");

        if (await context.MetricasProgramaMes.AnyAsync())
        {
            Console.WriteLine("??  Métricas ya existen, saltando...");
            return;
        }

        var programas = await context.Programas.ToListAsync();
        
        if (!programas.Any())
        {
            Console.WriteLine("?? No hay programas.");
            return;
        }

        var metricas = new List<MetricasProgramaMes>();
        var random = new Random(42);
        var fechaInicio = DateTime.Now.AddMonths(-6);

        for (int mes = 0; mes < 6; mes++)
        {
            var fecha = fechaInicio.AddMonths(mes);
            var anioMes = $"{fecha.Year:0000}-{fecha.Month:00}";

            foreach (var programa in programas)
            {
                // Calcular métricas desde actividades
                var planificadas = await context.Actividades
                    .Where(a => a.ProgramaId == programa.ProgramaId &&
                                a.FechaInicio.Year == fecha.Year &&
                                a.FechaInicio.Month == fecha.Month &&
                                !a.IsDeleted)
                    .CountAsync();

                var ejecutadas = await context.Actividades
                    .Where(a => a.ProgramaId == programa.ProgramaId &&
                                a.FechaInicio.Year == fecha.Year &&
                                a.FechaInicio.Month == fecha.Month &&
                                a.Estado == EstadoActividad.Realizada &&
                                !a.IsDeleted)
                    .CountAsync();

                var cumplimiento = planificadas > 0 ? (ejecutadas * 100.0m / planificadas) : 0;

                // Calcular asistencia promedio
                var asistencias = await context.Asistencias
                    .Include(a => a.Actividad)
                    .Where(a => a.Actividad.ProgramaId == programa.ProgramaId &&
                                a.Fecha.Year == fecha.Year &&
                                a.Fecha.Month == fecha.Month &&
                                !a.IsDeleted)
                    .ToListAsync();

                var totalAsistencias = asistencias.Count;
                var presentes = asistencias.Count(a => 
                    a.Estado == EstadoAsistencia.Presente || 
                    a.Estado == EstadoAsistencia.Tarde);
                var porcAsistencia = totalAsistencias > 0 ? (presentes * 100.0m / totalAsistencias) : 0;

                metricas.Add(new MetricasProgramaMes
                {
                    ProgramaId = programa.ProgramaId,
                    AnioMes = anioMes,
                    ActividadesPlanificadas = planificadas,
                    ActividadesEjecutadas = ejecutadas,
                    PorcCumplimiento = Math.Round(cumplimiento, 2),
                    RetrasoPromedioDias = random.Next(0, 5) + (decimal)random.NextDouble(),
                    PorcAsistenciaProm = Math.Round(porcAsistencia, 2),
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        await context.MetricasProgramaMes.AddRangeAsync(metricas);
        await context.SaveChangesAsync();

        Console.WriteLine($"? {metricas.Count} Métricas mensuales creadas");
    }
}
