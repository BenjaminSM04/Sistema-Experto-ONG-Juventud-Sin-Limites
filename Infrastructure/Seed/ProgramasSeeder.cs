using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder para Programas
/// </summary>
public static class ProgramasSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        Console.WriteLine("?? Seeding Programas...");

        if (await context.Programas.AnyAsync())
        {
            Console.WriteLine("??  Programas ya existen, saltando...");
            return;
        }

        var programas = new List<Programa>
        {
            new Programa
            {
                Clave = "EDV",
                Nombre = "Escuelas de Valores",
                Descripcion = "Programa de formación en valores para jóvenes",
                Estado = EstadoGeneral.Activo,
                InferenciaActiva = true,
                CreadoEn = DateTime.UtcNow
            },
            new Programa
            {
                Clave = "ACADEMIA",
                Nombre = "Academia de Liderazgo",
                Descripcion = "Desarrollo de habilidades de liderazgo",
                Estado = EstadoGeneral.Activo,
                InferenciaActiva = true,
                CreadoEn = DateTime.UtcNow
            },
            new Programa
            {
                Clave = "JUVENTUD_SEGURA",
                Nombre = "Juventud Segura",
                Descripcion = "Programa de prevención y seguridad",
                Estado = EstadoGeneral.Activo,
                InferenciaActiva = true,
                CreadoEn = DateTime.UtcNow
            },
            new Programa
            {
                Clave = "BERNABE",
                Nombre = "Programa Bernabé",
                Descripcion = "Acompañamiento integral a jóvenes",
                Estado = EstadoGeneral.Activo,
                InferenciaActiva = true,
                CreadoEn = DateTime.UtcNow
            }
        };

        await context.Programas.AddRangeAsync(programas);
        await context.SaveChangesAsync();

        Console.WriteLine($"? {programas.Count} Programas creados exitosamente");
    }
}
