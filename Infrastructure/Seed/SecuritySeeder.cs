using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder para Roles y Usuarios Administradores
/// </summary>
public static class SecuritySeeder
{
    public static async Task SeedRolesAsync(RoleManager<Rol> roleManager)
    {
        Console.WriteLine("?? Seeding Roles...");

        if (await roleManager.Roles.AnyAsync())
        {
            Console.WriteLine("??  Roles ya existen, saltando...");
            return;
        }

        var rolesData = new[]
        {
            new { Name = "Administrador", Descripcion = "Acceso total al sistema" },
            new { Name = "Coordinador", Descripcion = "Coordinador de programas" },
            new { Name = "Facilitador", Descripcion = "Facilitador de actividades" },
            new { Name = "Visualizador", Descripcion = "Solo lectura" }
        };

        foreach (var roleData in rolesData)
        {
            var rol = new Rol
            {
                Name = roleData.Name,
                Descripcion = roleData.Descripcion,
                CreadoEn = DateTime.UtcNow
            };

            var result = await roleManager.CreateAsync(rol);
            if (!result.Succeeded)
            {
                Console.WriteLine($"? Error creando rol {roleData.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        Console.WriteLine("? Roles creados exitosamente");
    }

    public static async Task SeedAdminUsersAsync(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        Console.WriteLine("?? Seeding Usuarios Administradores...");

        if (await userManager.Users.AnyAsync())
        {
            Console.WriteLine("??  Usuarios ya existen, saltando...");
            return;
        }

        var admins = new[]
        {
            new { Nombres = "Benjamín", Apellidos = "Saenz", Email = "benjaminspsn@outlook.com", Nac = new DateTime(1990, 1, 1) },
            new { Nombres = "Brisa", Apellidos = "Criales", Email = "maytasindel@gmail.com", Nac = new DateTime(1995, 1, 1) }
        };

        var usuariosCreados = new List<Usuario>();

        foreach (var admin in admins)
        {
            var persona = new Persona
            {
                Nombres = admin.Nombres,
                Apellidos = admin.Apellidos,
                FechaNacimiento = admin.Nac,
                Telefono = "0000-0000",
                CreadoEn = DateTime.UtcNow
            };

            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var usuario = new Usuario
            {
                PersonaId = persona.PersonaId,
                UserName = admin.Email,
                Email = admin.Email,
                EmailConfirmed = true,
                Estado = EstadoGeneral.Activo,
                MustChangePassword = false,
                TwoFactorEnabled = true,
                CreatedBy = "Sistema",
                CreatedAtUtc = DateTime.UtcNow,
                CreadoEn = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(usuario, "Administrador@2025!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(usuario, "Administrador");
                usuariosCreados.Add(usuario);
                Console.WriteLine($"? Usuario administrador creado ({admin.Email})");
            }
            else
            {
                Console.WriteLine($"? Error creando usuario {admin.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // Asignar programas a los admins (si existen)
        var programas = await context.Programas
            .Where(p => p.Clave == "EDV" || p.Clave == "ACADEMIA")
            .ToListAsync();

        foreach (var prog in programas)
        {
            foreach (var usuario in usuariosCreados)
            {
                context.UsuarioProgramas.Add(new UsuarioPrograma
                {
                    UsuarioId = usuario.Id,
                    ProgramaId = prog.ProgramaId,
                    Desde = new DateTime(2025, 1, 1),
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("? Programas asignados a administradores");
    }
}
