using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder para Personas y Participantes
/// </summary>
public static class ParticipantesSeeder
{
    private static readonly string[] Nombres = {
        "Ana Sofía", "Luis Alberto", "Camila", "Diego", "Mariana", "Javier",
        "Valentina", "Santiago", "Isabella", "Mateo", "Sofía", "Sebastián",
        "Lucía", "Alejandro", "Victoria", "Daniel", "Emma", "Gabriel",
        "Martina", "Nicolás", "Paula", "Andrés", "Carolina", "Felipe",
        "Daniela", "Ricardo", "Gabriela", "Miguel", "Alejandra", "Fernando",
        "Laura", "Cristian", "Natalia", "Jorge", "Andrea", "Roberto",
        "María José", "Carlos", "Juliana", "Pedro", "Catalina", "Manuel",
        "Valeria", "Raúl", "Paola", "Eduardo", "Diana", "Rodrigo",
        "Claudia", "Gustavo", "Patricia", "Hernán", "Sandra", "Óscar",
        "Beatriz", "Antonio", "Mónica", "Francisco", "Adriana", "José",
        "Silvia", "Álvaro", "Teresa", "Iván", "Gloria", "Leonardo",
        "Rosa", "Sergio", "Inés", "Tomás", "Elvira", "Ángel",
        "Carmen", "Víctor", "Pilar", "Esteban", "Dolores", "Alberto",
        "Amparo", "Julio", "Concepción", "Ramón", "Mercedes", "Pablo"
    };

    private static readonly string[] Apellidos = {
        "García", "Rodríguez", "Martínez", "Fernández", "López", "González",
        "Pérez", "Sánchez", "Ramírez", "Torres", "Flores", "Rivera",
        "Gómez", "Díaz", "Cruz", "Reyes", "Morales", "Jiménez",
        "Hernández", "Ruiz", "Mendoza", "Álvarez", "Castillo", "Romero",
        "Ortiz", "Silva", "Vargas", "Castro", "Ramos", "Vega",
        "Moreno", "Guerrero", "Medina", "Salazar", "Rojas", "Contreras",
        "Gutiérrez", "Pinto", "Vásquez", "Núñez", "Herrera", "Campos",
        "Cortés", "Aguilar", "Navarro", "Mendez", "Serrano", "Valencia",
        "Molina", "Bravo", "Peña", "Cabrera", "Fuentes", "Espinoza"
    };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        Console.WriteLine("?? Seeding Personas y Participantes...");

        // Verificar si ya existen participantes (no personas, porque hay admins)
        if (await context.Participantes.AnyAsync())
        {
            Console.WriteLine("??  Participantes ya existen, saltando...");
            return;
        }

        var random = new Random(42); // Seed fijo para reproducibilidad
        var personas = new List<Persona>();
        var participantesData = new List<(Persona Persona, DateTime FechaAlta)>();

        // ========== GENERAR 80 PARTICIPANTES ==========
        // 40 para EDV (edades 13-25)
        for (int i = 0; i < 40; i++)
        {
            var persona = GenerarPersona(random, 13, 25);
            personas.Add(persona);
            participantesData.Add((persona, DateTime.Now.AddDays(-random.Next(180, 730))));
        }

        // 40 para ACADEMIA (edades 15-28)
        for (int i = 0; i < 40; i++)
        {
            var persona = GenerarPersona(random, 15, 28);
            personas.Add(persona);
            participantesData.Add((persona, DateTime.Now.AddDays(-random.Next(180, 730))));
        }

        // Guardar personas primero
        await context.Personas.AddRangeAsync(personas);
        await context.SaveChangesAsync();

        Console.WriteLine($"? {personas.Count} Personas creadas");

        // Crear participantes con las personas ya guardadas (tienen IDs)
        var participantes = new List<Participante>();

        for (int i = 0; i < participantesData.Count; i++)
        {
            var (persona, fechaAlta) = participantesData[i];
            
            // 90% activos, 10% inactivos
            var estado = random.Next(100) < 90 ? EstadoGeneral.Activo : EstadoGeneral.Inactivo;

            var participante = new Participante
            {
                PersonaId = persona.PersonaId,
                Estado = estado,
                FechaAlta = fechaAlta,
                CreadoEn = DateTime.UtcNow
            };

            participantes.Add(participante);
        }

        await context.Participantes.AddRangeAsync(participantes);
        await context.SaveChangesAsync();

        Console.WriteLine($"? {participantes.Count} Participantes creados");
        Console.WriteLine($"   - Activos: {participantes.Count(p => p.Estado == EstadoGeneral.Activo)}");
        Console.WriteLine($"   - Inactivos: {participantes.Count(p => p.Estado == EstadoGeneral.Inactivo)}");
    }

    private static Persona GenerarPersona(Random random, int edadMin, int edadMax)
    {
        var nombre = Nombres[random.Next(Nombres.Length)];
        var apellido1 = Apellidos[random.Next(Apellidos.Length)];
        var apellido2 = Apellidos[random.Next(Apellidos.Length)];

        var edad = random.Next(edadMin, edadMax + 1);
        var nacimiento = DateTime.Now.AddYears(-edad).AddDays(random.Next(-180, 180));

        var telefono = $"{random.Next(6, 10)}{random.Next(100, 1000)}-{random.Next(1000, 10000)}";

        return new Persona
        {
            Nombres = nombre,
            Apellidos = $"{apellido1} {apellido2}",
            FechaNacimiento = nacimiento,
            Telefono = telefono,
            CreadoEn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Obtiene participantes por índice de programa (0 = EDV, 1 = ACADEMIA)
    /// </summary>
    public static async Task<List<Participante>> ObtenerParticipantesPorProgramaAsync(
        ApplicationDbContext context, 
        int programaIndex)
    {
        var participantes = await context.Participantes
            .Include(p => p.Persona)
            .OrderBy(p => p.ParticipanteId)
            .ToListAsync();

        // Primeros 40 son EDV, siguientes 40 son ACADEMIA
        return programaIndex switch
        {
            0 => participantes.Take(40).ToList(),
            1 => participantes.Skip(40).Take(40).ToList(),
            _ => participantes.ToList()
        };
    }
}
