using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Orquestador principal de todos los seeders de la base de datos
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context, 
        UserManager<Usuario> userManager, 
        RoleManager<Rol> roleManager)
    {
        // Verificar que la base de datos esté creada
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine("🌱 ========================================");
        Console.WriteLine("🌱 INICIANDO PROCESO DE SEEDING");
        Console.WriteLine("🌱 ========================================\n");

        try
        {
            // ========== 1) ROLES ==========
            await SecuritySeeder.SeedRolesAsync(roleManager);

            // ========== 2) PROGRAMAS (antes de usuarios para asignarlos) ==========
            await ProgramasSeeder.SeedAsync(context);

            // ========== 3) USUARIOS ADMINISTRADORES ==========
            await SecuritySeeder.SeedAdminUsersAsync(context, userManager);

            // ========== 4) CONFIGURACIÓN DEL MOTOR ==========
            await MotorSeeder.SeedConfiguracionAsync(context);

            // ========== 5) REGLAS Y PARÁMETROS ==========
            await MotorSeeder.SeedReglasAsync(context);

            // ========== 6) PERSONAS Y PARTICIPANTES ==========
            await ParticipantesSeeder.SeedAsync(context);

            // ========== 7) ACTIVIDADES ==========
            await ActividadesSeeder.SeedAsync(context);

            // ========== 8) INSCRIPCIONES A ACTIVIDADES ==========
            await InscripcionesSeeder.SeedAsync(context);

            // ========== 9) ASISTENCIAS ==========
            await AsistenciasSeeder.SeedAsync(context);

            // ========== 10) EVIDENCIAS ==========
            await BISeeder.SeedEvidenciasAsync(context);

            // ========== 11) MÉTRICAS MENSUALES ==========
            await BISeeder.SeedMetricasAsync(context);

            // ========== 12) POA DINÁMICO ==========
            await POASeeder.SeedAsync(context);

            // ========== 13) DICCIONARIO DE OBSERVACIONES ==========
            await MotorSeeder.SeedDiccionarioObservacionesAsync(context);

            Console.WriteLine("\n🌱 ========================================");
            Console.WriteLine("✅ SEEDING COMPLETADO EXITOSAMENTE!");
            Console.WriteLine("🌱 ========================================\n");

            // Mostrar resumen
            await MostrarResumenAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERROR EN SEEDING: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Muestra un resumen de los datos creados
    /// </summary>
    private static async Task MostrarResumenAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📊 RESUMEN DE DATOS CREADOS:");
        Console.WriteLine("─────────────────────────────────────────");

        var programas = await context.Programas.CountAsync(p => !p.IsDeleted);
        var personas = await context.Personas.CountAsync(p => !p.IsDeleted);
        var participantes = await context.Participantes.CountAsync(p => !p.IsDeleted);
        var actividades = await context.Actividades.CountAsync(a => !a.IsDeleted);
        var inscripciones = await context.ActividadParticipantes.CountAsync(ap => !ap.IsDeleted);
        var asistencias = await context.Asistencias.CountAsync(a => !a.IsDeleted);
        var reglas = await context.Reglas.CountAsync(r => !r.IsDeleted);
        var evidencias = await context.EvidenciaActividades.CountAsync(e => !e.IsDeleted);

        Console.WriteLine($"   📚 Programas:      {programas}");
        Console.WriteLine($"   👤 Personas:       {personas}");
        Console.WriteLine($"   👥 Participantes:  {participantes}");
        Console.WriteLine($"   📅 Actividades:    {actividades}");
        Console.WriteLine($"   📝 Inscripciones:  {inscripciones}");
        Console.WriteLine($"   ✅ Asistencias:    {asistencias}");
        Console.WriteLine($"   📏 Reglas:         {reglas}");
        Console.WriteLine($"   📎 Evidencias:     {evidencias}");
        Console.WriteLine("─────────────────────────────────────────\n");
    }
}

