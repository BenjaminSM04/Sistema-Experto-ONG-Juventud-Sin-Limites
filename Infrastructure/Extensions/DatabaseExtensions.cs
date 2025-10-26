using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// Aplica migraciones pendientes y ejecuta el seeder de datos iniciales
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Rol>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Aplicando migraciones de base de datos...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas exitosamente");

            logger.LogInformation("Ejecutando seeder de datos iniciales...");
            await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
            logger.LogInformation("Datos iniciales cargados exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inicializar la base de datos");
            throw;
        }
    }

    /// <summary>
    /// Solo aplica migraciones sin ejecutar el seeder
    /// </summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Aplicando migraciones de base de datos...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al aplicar migraciones");
            throw;
        }
    }
}
