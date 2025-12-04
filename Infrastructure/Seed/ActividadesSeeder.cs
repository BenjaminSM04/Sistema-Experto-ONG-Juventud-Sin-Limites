using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder para Actividades
/// </summary>
public static class ActividadesSeeder
{
    private static readonly string[] TitulosEDV = {
        "Taller de Valores", "Formación en Liderazgo", "Convivencia Escolar",
        "Prevención de Violencia", "Cultura de Paz", "Resolución de Conflictos",
        "Comunicación Asertiva", "Trabajo en Equipo", "Empatía y Respeto"
    };

    private static readonly string[] TitulosAcademia = {
        "Mentoría Individual", "Coaching Grupal", "Desarrollo Personal",
        "Planificación de Vida", "Habilidades Blandas", "Oratoria",
        "Gestión del Tiempo", "Inteligencia Emocional", "Liderazgo Juvenil"
    };

    private static readonly string[] Lugares = { 
        "Salón A", "Salón B", "Auditorio", "Aula 1", "Aula 2", "Patio Central", "Biblioteca" 
    };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        Console.WriteLine("?? Seeding Actividades...");

        if (await context.Actividades.AnyAsync())
        {
            Console.WriteLine("??  Actividades ya existen, saltando...");
            return;
        }

        var programas = await context.Programas.ToListAsync();
        
        if (!programas.Any())
        {
            Console.WriteLine("?? No hay programas. Ejecute ProgramasSeeder primero.");
            return;
        }

        var actividades = new List<Actividad>();
        var random = new Random(42);
        var fechaInicio = DateTime.Now.AddMonths(-6);

        // EDV: 60 actividades
        var progEDV = programas.FirstOrDefault(p => p.Clave == "EDV");
        if (progEDV != null)
        {
            var actividadesEDV = GenerarActividades(
                progEDV.ProgramaId, 
                TitulosEDV, 
                "Sesión de formación en valores para jóvenes",
                60, 3, 18, 20, random, fechaInicio);
            actividades.AddRange(actividadesEDV);
        }

        // ACADEMIA: 40 actividades
        var progACADEMIA = programas.FirstOrDefault(p => p.Clave == "ACADEMIA");
        if (progACADEMIA != null)
        {
            var actividadesAcademia = GenerarActividades(
                progACADEMIA.ProgramaId, 
                TitulosAcademia, 
                "Desarrollo de habilidades de liderazgo",
                40, 4, 17, 19, random, fechaInicio);
            actividades.AddRange(actividadesAcademia);
        }

        // JUVENTUD SEGURA: 30 actividades
        var progJuventud = programas.FirstOrDefault(p => p.Clave == "JUVENTUD_SEGURA");
        if (progJuventud != null)
        {
            var actividadesJuventud = GenerarActividades(
                progJuventud.ProgramaId, 
                new[] { "Prevención y Seguridad" }, 
                "Actividad de prevención y seguridad para jóvenes",
                30, 6, 16, 18, random, fechaInicio);
            actividades.AddRange(actividadesJuventud);
        }

        // BERNABÉ: 20 actividades
        var progBernabe = programas.FirstOrDefault(p => p.Clave == "BERNABE");
        if (progBernabe != null)
        {
            var actividadesBernabe = GenerarActividades(
                progBernabe.ProgramaId, 
                new[] { "Acompañamiento Integral" }, 
                "Sesión de acompañamiento integral a jóvenes",
                20, 9, 15, 17, random, fechaInicio);
            actividades.AddRange(actividadesBernabe);
        }

        await context.Actividades.AddRangeAsync(actividades);
        await context.SaveChangesAsync();

        var retrasadas = actividades.Count(a => 
            a.Estado == EstadoActividad.Planificada && 
            a.FechaInicio < DateTime.Now.AddDays(-7));

        Console.WriteLine($"? {actividades.Count} Actividades creadas");
        Console.WriteLine($"   - Realizadas: {actividades.Count(a => a.Estado == EstadoActividad.Realizada)}");
        Console.WriteLine($"   - Planificadas: {actividades.Count(a => a.Estado == EstadoActividad.Planificada)}");
        Console.WriteLine($"   - Retrasadas (para alertas): {retrasadas}");
    }

    private static List<Actividad> GenerarActividades(
        int programaId,
        string[] titulos,
        string descripcionBase,
        int cantidad,
        int diasIntervalo,
        int horaInicio,
        int horaFin,
        Random random,
        DateTime fechaInicio)
    {
        var actividades = new List<Actividad>();

        for (int i = 0; i < cantidad; i++)
        {
            var diasDesdeInicio = i * diasIntervalo;
            var fechaActividad = fechaInicio.AddDays(diasDesdeInicio);

            // Ajustar a día laboral
            while (fechaActividad.DayOfWeek == DayOfWeek.Saturday || fechaActividad.DayOfWeek == DayOfWeek.Sunday)
            {
                fechaActividad = fechaActividad.AddDays(1);
            }

            var titulo = titulos[i % titulos.Length] + $" {i + 1:000}";

            // Determinar estado
            EstadoActividad estado;
            if (fechaActividad < DateTime.Now.AddDays(-10) && i % 8 == 0)
            {
                // Actividades retrasadas para generar alertas
                estado = EstadoActividad.Planificada;
            }
            else if (fechaActividad < DateTime.Now.AddDays(-7))
            {
                estado = random.Next(100) < 85 ? EstadoActividad.Realizada : EstadoActividad.Planificada;
            }
            else
            {
                estado = EstadoActividad.Planificada;
            }

            var tipo = random.Next(100) < 70 
                ? TipoActividad.Taller 
                : (random.Next(100) < 50 ? TipoActividad.Capacitacion : TipoActividad.Evento);

            actividades.Add(new Actividad
            {
                ProgramaId = programaId,
                Titulo = titulo,
                Descripcion = $"{descripcionBase} - {titulo}",
                FechaInicio = fechaActividad.Date.AddHours(horaInicio),
                FechaFin = fechaActividad.Date.AddHours(horaFin),
                Lugar = Lugares[random.Next(Lugares.Length)],
                Tipo = tipo,
                Estado = estado,
                CreadoEn = DateTime.UtcNow
            });
        }

        return actividades;
    }
}
