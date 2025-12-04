using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder para Inscripciones de Participantes a Actividades
/// </summary>
public static class InscripcionesSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        Console.WriteLine("?? Seeding Inscripciones a Actividades...");

        if (await context.ActividadParticipantes.AnyAsync())
        {
            Console.WriteLine("??  Inscripciones ya existen, saltando...");
            return;
        }

        var actividades = await context.Actividades
            .Include(a => a.Programa)
            .ToListAsync();

        var participantes = await context.Participantes
            .Include(p => p.Persona)
            .OrderBy(p => p.ParticipanteId)
            .ToListAsync();

        if (!actividades.Any() || !participantes.Any())
        {
            Console.WriteLine("?? No hay actividades o participantes. Ejecute los seeders previos.");
            return;
        }

        var random = new Random(42);
        var inscripciones = new List<ActividadParticipante>();

        // Dividir participantes: primeros 40 para EDV, siguientes 40 para ACADEMIA
        var partEDV = participantes.Take(40).ToList();
        var partACADEMIA = participantes.Skip(40).Take(40).ToList();

        // ========== EDV ==========
        var actividadesEDV = actividades
            .Where(a => a.Programa?.Clave == "EDV")
            .OrderBy(a => a.FechaInicio)
            .ToList();

        // Primeras 3 actividades sin inscriptos (para alertas INFO)
        var actividadesEDVConInscripciones = actividadesEDV.Skip(3).ToList();
        
        Console.WriteLine($"?? {Math.Min(3, actividadesEDV.Count)} actividades EDV sin inscriptos (alertas INFO)");

        foreach (var actividad in actividadesEDVConInscripciones)
        {
            var cantidadParticipantes = random.Next(25, 36);
            var seleccionados = partEDV.OrderBy(_ => random.Next()).Take(cantidadParticipantes);

            foreach (var participante in seleccionados)
            {
                inscripciones.Add(new ActividadParticipante
                {
                    ActividadId = actividad.ActividadId,
                    ParticipanteId = participante.ParticipanteId,
                    Rol = RolParticipante.Asistente,
                    Estado = random.Next(100) < 95 ? EstadoInscripcion.Inscrito : EstadoInscripcion.Retirado,
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        // ========== ACADEMIA ==========
        var actividadesACADEMIA = actividades
            .Where(a => a.Programa?.Clave == "ACADEMIA")
            .OrderBy(a => a.FechaInicio)
            .ToList();

        // Primeras 2 actividades sin inscriptos (para alertas INFO)
        var actividadesACADEMIAConInscripciones = actividadesACADEMIA.Skip(2).ToList();
        
        Console.WriteLine($"?? {Math.Min(2, actividadesACADEMIA.Count)} actividades ACADEMIA sin inscriptos (alertas INFO)");

        foreach (var actividad in actividadesACADEMIAConInscripciones)
        {
            var cantidadParticipantes = random.Next(15, 26);
            var seleccionados = partACADEMIA.OrderBy(_ => random.Next()).Take(cantidadParticipantes);

            foreach (var participante in seleccionados)
            {
                inscripciones.Add(new ActividadParticipante
                {
                    ActividadId = actividad.ActividadId,
                    ParticipanteId = participante.ParticipanteId,
                    Rol = RolParticipante.Asistente,
                    Estado = random.Next(100) < 93 ? EstadoInscripcion.Inscrito : EstadoInscripcion.Retirado,
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        // ========== OTROS PROGRAMAS ==========
        var actividadesOtros = actividades
            .Where(a => a.Programa?.Clave != "EDV" && a.Programa?.Clave != "ACADEMIA")
            .ToList();

        var todosParticipantes = partEDV.Concat(partACADEMIA).ToList();

        foreach (var actividad in actividadesOtros)
        {
            var cantidadParticipantes = random.Next(10, 21);
            var seleccionados = todosParticipantes.OrderBy(_ => random.Next()).Take(cantidadParticipantes);

            foreach (var participante in seleccionados)
            {
                inscripciones.Add(new ActividadParticipante
                {
                    ActividadId = actividad.ActividadId,
                    ParticipanteId = participante.ParticipanteId,
                    Rol = RolParticipante.Asistente,
                    Estado = random.Next(100) < 90 ? EstadoInscripcion.Inscrito : EstadoInscripcion.Retirado,
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        await context.ActividadParticipantes.AddRangeAsync(inscripciones);
        await context.SaveChangesAsync();

        Console.WriteLine($"? {inscripciones.Count} Inscripciones creadas");
        Console.WriteLine($"   - Inscritos: {inscripciones.Count(i => i.Estado == EstadoInscripcion.Inscrito)}");
        Console.WriteLine($"   - Retirados: {inscripciones.Count(i => i.Estado == EstadoInscripcion.Retirado)}");
    }
}
