using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<Usuario> userManager, RoleManager<Rol> roleManager)
    {
        // Verificar que la base de datos esté creada
        await context.Database.EnsureCreatedAsync();

        // Seed de Roles usando RoleManager
 if (!await roleManager.Roles.AnyAsync())
        {
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
   Console.WriteLine($"? Error creating role {roleData.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
          }

        Console.WriteLine("? Roles creados exitosamente");
  }

  // Crear usuario administrador por defecto
     if (!await userManager.Users.AnyAsync())
    {
            // Primero crear la persona
      var personaAdmin = new Persona
 {
   Nombres = "Administrador",
     Apellidos = "Del Sistema",
 FechaNacimiento = new DateTime(1990, 1, 1),
      Telefono = "0000-0000",
       CreadoEn = DateTime.UtcNow
            };

            context.Personas.Add(personaAdmin);
     await context.SaveChangesAsync();

      // Crear el usuario
   var adminUser = new Usuario
     {
 PersonaId = personaAdmin.PersonaId,
  UserName = "admin@ong.com",
          Email = "admin@ong.com",
EmailConfirmed = true,
Estado = EstadoGeneral.Activo,
       CreadoEn = DateTime.UtcNow
     };

      // Crear usuario con password
    var result = await userManager.CreateAsync(adminUser, "Admin@123");
     if (result.Succeeded)
            {
           // Asignar rol de Administrador
       await userManager.AddToRoleAsync(adminUser, "Administrador");
          Console.WriteLine("? Usuario administrador creado (admin@ong.com / Admin@123)");
            }
     else
        {
         Console.WriteLine($"? Error creating admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
       }
        }

        // Seed de Programas
  if (!await context.Programas.AnyAsync())
      {
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
}

   // Seed de Configuración del Motor
  if (!await context.ConfiguracionesMotor.AnyAsync())
            {
var configuraciones = new List<Domain.Config.ConfiguracionMotor>
        {
         new Domain.Config.ConfiguracionMotor
         {
        Clave = "ASISTENCIA_MIN_PORCENTAJE",
       Valor = "75",
   Descripcion = "Porcentaje mínimo de asistencia requerido",
       Version = 1,
  CreadoEn = DateTime.UtcNow
    },
          new Domain.Config.ConfiguracionMotor
   {
 Clave = "DIAS_ALERTA_INASISTENCIA",
 Valor = "7",
      Descripcion = "Días consecutivos de inasistencia para generar alerta",
   Version = 1,
  CreadoEn = DateTime.UtcNow
  },
    new Domain.Config.ConfiguracionMotor
  {
       Clave = "UMBRAL_RIESGO_BAJO",
  Valor = "30",
  Descripcion = "Score máximo para riesgo bajo",
    Version = 1,
        CreadoEn = DateTime.UtcNow
},
     new Domain.Config.ConfiguracionMotor
     {
 Clave = "UMBRAL_RIESGO_MEDIO",
    Valor = "60",
     Descripcion = "Score máximo para riesgo medio",
   Version = 1,
    CreadoEn = DateTime.UtcNow
  },
      new Domain.Config.ConfiguracionMotor
       {
    Clave = "MOTOR_EJECUCION_AUTO",
          Valor = "true",
  Descripcion = "Ejecutar motor de inferencia automáticamente",
     Version = 1,
  CreadoEn = DateTime.UtcNow
        },
    new Domain.Config.ConfiguracionMotor
 {
  Clave = "FRECUENCIA_EJECUCION_HORAS",
 Valor = "24",
    Descripcion = "Frecuencia de ejecución del motor en horas",
    Version = 1,
     CreadoEn = DateTime.UtcNow
       }
     };

 await context.ConfiguracionesMotor.AddRangeAsync(configuraciones);
       await context.SaveChangesAsync();
  }

        // Seed de Reglas Básicas
        if (!await context.Reglas.AnyAsync())
        {
   var reglas = new List<Domain.Motor.Regla>
       {
           new Domain.Motor.Regla
      {
   Clave = "INASISTENCIA_CONSECUTIVA",
      Nombre = "Inasistencia Consecutiva",
  Descripcion = "Detecta participantes con múltiples inasistencias consecutivas",
    Severidad = Severidad.Alta,
    Objetivo = ObjetivoRegla.Participante,
     Activa = true,
 Prioridad = 100,
  Version = 1,
       CreadoEn = DateTime.UtcNow
        },
     new Domain.Motor.Regla
  {
           Clave = "BAJA_ASISTENCIA_GENERAL",
  Nombre = "Baja Asistencia General",
      Descripcion = "Detecta participantes con porcentaje de asistencia por debajo del umbral",
          Severidad = Severidad.Alta,
      Objetivo = ObjetivoRegla.Participante,
   Activa = true,
       Prioridad = 90,
     Version = 1,
        CreadoEn = DateTime.UtcNow
   },
    new Domain.Motor.Regla
       {
  Clave = "ACTIVIDAD_SIN_ASISTENTES",
       Nombre = "Actividad Sin Asistentes",
            Descripcion = "Detecta actividades planificadas sin participantes inscritos",
        Severidad = Severidad.Info,
   Objetivo = ObjetivoRegla.Actividad,
    Activa = true,
    Prioridad = 50,
     Version = 1,
       CreadoEn = DateTime.UtcNow
},
          new Domain.Motor.Regla
  {
   Clave = "RETRASO_ACTIVIDAD",
 Nombre = "Retraso en Ejecución de Actividad",
     Descripcion = "Detecta actividades planificadas que no se han ejecutado en la fecha prevista",
      Severidad = Severidad.Alta,
          Objetivo = ObjetivoRegla.Actividad,
      Activa = true,
Prioridad = 80,
               Version = 1,
  CreadoEn = DateTime.UtcNow
            },
   new Domain.Motor.Regla
    {
      Clave = "BAJO_CUMPLIMIENTO_POA",
      Nombre = "Bajo Cumplimiento de POA",
   Descripcion = "Detecta programas con bajo porcentaje de cumplimiento mensual",
       Severidad = Severidad.Critica,
   Objetivo = ObjetivoRegla.Programa,
     Activa = true,
         Prioridad = 100,
     Version = 1,
     CreadoEn = DateTime.UtcNow
       }
  };

      await context.Reglas.AddRangeAsync(reglas);
await context.SaveChangesAsync();
  }

        Console.WriteLine("? Database seeded successfully!");
    }
}
