using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.BI;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Config;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<Usuario> userManager, RoleManager<Rol> roleManager)
    {
        // Verificar que la base de datos esté creada
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine("🌱 Iniciando proceso de Seeding...");

        // ========== 1) ROLES ========== 
        await SeedRolesAsync(roleManager);

        // ========== 2) USUARIO ADMIN ==========
        await SeedAdminUserAsync(context, userManager);

        // ========== 3) PROGRAMAS ========== 
        await SeedProgramasAsync(context);

        // ========== 4) CONFIGURACIÓN DEL MOTOR ========== 
        await SeedConfiguracionMotorAsync(context);

        // ========== 5) REGLAS Y PARÁMETROS ==========
        await SeedReglasYParametrosAsync(context);

        // ========== 6) PERSONAS Y PARTICIPANTES ==========
        await SeedPersonasYParticipantesAsync(context);

        // ========== 7) ACTIVIDADES OCTUBRE 2025 ==========
        await SeedActividadesAsync(context);

        // ========== 8) INSCRIPCIONES A ACTIVIDADES ==========
        await SeedActividadParticipantesAsync(context);

        // ========== 9) ASISTENCIAS (para generar alertas) ==========
        await SeedAsistenciasAsync(context);

        // ========== 10) EVIDENCIAS DE ACTIVIDADES ==========
        await SeedEvidenciasAsync(context);

        // ========== 11) MÉTRICAS MENSUALES ==========
        await SeedMetricasProgramaMesAsync(context);

        // ========== 12) POA DINÁMICO (OPCIONAL) ==========
        await SeedPOADinamicoAsync(context);

        // ========== 13) DICCIONARIO DE OBSERVACIONES (OPCIONAL) ==========
        await SeedDiccionarioObservacionesAsync(context);

        Console.WriteLine("✅ Seeding completado exitosamente!");
    }

    // ========================================
    // MÉTODOS AUXILIARES DE SEEDING
    // ========================================

    private static async Task SeedRolesAsync(RoleManager<Rol> roleManager)
    {
        Console.WriteLine("📋 Seeding Roles...");

        if (await roleManager.Roles.AnyAsync())
        {
            Console.WriteLine("⏭️  Roles ya existen, saltando...");
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
                Console.WriteLine($"❌ Error creando rol {roleData.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        Console.WriteLine("✅ Roles creados exitosamente");
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        Console.WriteLine("👤 Seeding Usuarios Administradores...");

        if (await userManager.Users.AnyAsync())
        {
            Console.WriteLine("⏭️  Usuarios ya existen, saltando...");
            return;
        }

        // ========== USUARIO ADMIN 1: Benjamín ==========
        var personaAdmin1 = new Persona
        {
            Nombres = "Benjamín",
            Apellidos = "Saenz",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Telefono = "0000-0000",
            CreadoEn = DateTime.UtcNow
        };

        context.Personas.Add(personaAdmin1);
        await context.SaveChangesAsync();

        var adminUser1 = new Usuario
        {
            PersonaId = personaAdmin1.PersonaId,
            UserName = "benjaminspsn@outlook.com",
            Email = "benjaminspsn@outlook.com",
            EmailConfirmed = true,
            Estado = EstadoGeneral.Activo,
            MustChangePassword = false,
            TwoFactorEnabled = true,
            CreatedBy = "Sistema",
            CreatedAtUtc = DateTime.UtcNow,
            CreadoEn = DateTime.UtcNow
        };

        var result1 = await userManager.CreateAsync(adminUser1, "Administrador@2025!");
        if (result1.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser1, "Administrador");
            Console.WriteLine("✅ Usuario administrador creado (benjaminspsn@outlook.com / Administrador@2025!)");
        }
        else
        {
            Console.WriteLine($"❌ Error creando admin user 1: {string.Join(", ", result1.Errors.Select(e => e.Description))}");
        }

        // ========== USUARIO ADMIN 2: Brisa ==========
        var personaAdmin2 = new Persona
        {
            Nombres = "Brisa",
            Apellidos = "Criales",
            FechaNacimiento = new DateTime(1995, 1, 1),
            Telefono = "0000-0000",
            CreadoEn = DateTime.UtcNow
        };

        context.Personas.Add(personaAdmin2);
        await context.SaveChangesAsync();

        var adminUser2 = new Usuario
        {
            PersonaId = personaAdmin2.PersonaId,
            UserName = "maytasindel@gmail.com",
            Email = "maytasindel@gmail.com",
            EmailConfirmed = true,
            Estado = EstadoGeneral.Activo,
            MustChangePassword = false,
            TwoFactorEnabled = true,
            CreatedBy = "Sistema",
            CreatedAtUtc = DateTime.UtcNow,
            CreadoEn = DateTime.UtcNow
        };

        var result2 = await userManager.CreateAsync(adminUser2, "Administrador@2025!");
        if (result2.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser2, "Administrador");
            Console.WriteLine("✅ Usuario administrador creado (maytasindel@gmail.com / Administrador@2025!)");
        }
        else
        {
            Console.WriteLine($"❌ Error creando admin user 2: {string.Join(", ", result2.Errors.Select(e => e.Description))}");
        }

        // ========== ASIGNAR PROGRAMAS A AMBOS ADMINS ==========
        var programas = await context.Programas.Where(p => p.Clave == "EDV" || p.Clave == "ACADEMIA").ToListAsync();
        
        foreach (var prog in programas)
        {
            // Asignar a Benjamín
            context.UsuarioProgramas.Add(new UsuarioPrograma
            {
                UsuarioId = adminUser1.Id,
                ProgramaId = prog.ProgramaId,
                Desde = new DateTime(2025, 10, 1),
                CreadoEn = DateTime.UtcNow
            });

            // Asignar a Brisa
            context.UsuarioProgramas.Add(new UsuarioPrograma
            {
                UsuarioId = adminUser2.Id,
                ProgramaId = prog.ProgramaId,
                Desde = new DateTime(2025, 10, 1),
                CreadoEn = DateTime.UtcNow
            });
        }
        
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Programas asignados a ambos administradores");
    }

    private static async Task SeedProgramasAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📚 Seeding Programas...");

        if (await context.Programas.AnyAsync())
        {
            Console.WriteLine("⏭️  Programas ya existen, saltando...");
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

        Console.WriteLine("✅ Programas creados exitosamente");
    }

    private static async Task SeedConfiguracionMotorAsync(ApplicationDbContext context)
    {
        Console.WriteLine("⚙️  Seeding Configuración del Motor...");

        if (await context.ConfiguracionesMotor.AnyAsync())
        {
            Console.WriteLine("⏭️  Configuraciones ya existen, saltando...");
            return;
        }

        var configuraciones = new List<ConfiguracionMotor>
        {
    new ConfiguracionMotor
    {
     Clave = "ASISTENCIA_MIN_PORCENTAJE",
        Valor = "75",
        Descripcion = "Porcentaje mínimo de asistencia requerido",
  Version = 1,
          CreadoEn = DateTime.UtcNow
    },
            new ConfiguracionMotor
   {
     Clave = "DIAS_ALERTA_INASISTENCIA",
       Valor = "7",
  Descripcion = "Días consecutivos de inasistencia para generar alerta",
             Version = 1,
     CreadoEn = DateTime.UtcNow
            },
            new ConfiguracionMotor
            {
Clave = "UMBRAL_RIESGO_BAJO",
          Valor = "30",
                Descripcion = "Score máximo para riesgo bajo",
     Version = 1,
     CreadoEn = DateTime.UtcNow
 },
            new ConfiguracionMotor
  {
         Clave = "UMBRAL_RIESGO_MEDIO",
    Valor = "60",
           Descripcion = "Score máximo para riesgo medio",
                Version = 1,
    CreadoEn = DateTime.UtcNow
 },
            new ConfiguracionMotor
      {
        Clave = "MOTOR_EJECUCION_AUTO",
      Valor = "true",
         Descripcion = "Ejecutar motor de inferencia automáticamente",
        Version = 1,
CreadoEn = DateTime.UtcNow
 },
            new ConfiguracionMotor
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

        // Overrides por programa
        var programas = await context.Programas.ToListAsync();
        var progAcademia = programas.First(p => p.Clave == "ACADEMIA");
        var progEDV = programas.First(p => p.Clave == "EDV");

        var overrides = new List<ConfiguracionMotorOverride>
        {
         new ConfiguracionMotorOverride
        {
    ProgramaId = progAcademia.ProgramaId,
       Clave = "MOTOR_EJECUCION_AUTO",
       Valor = "true",
                Descripcion = "Override para ACADEMIA",
     Version = 1,
                CreadoEn = DateTime.UtcNow
    },
  new ConfiguracionMotorOverride
      {
         ProgramaId = progEDV.ProgramaId,
           Clave = "FRECUENCIA_EJECUCION_HORAS",
   Valor = "12",
             Descripcion = "Override para EDV",
                Version = 1,
         CreadoEn = DateTime.UtcNow
        }
        };

        await context.ConfiguracionMotorOverrides.AddRangeAsync(overrides);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Configuraciones del Motor creadas exitosamente");
    }

    private static async Task SeedReglasYParametrosAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📏 Seeding Reglas y Parámetros...");

        if (await context.Reglas.AnyAsync())
        {
            Console.WriteLine("⏭️  Reglas ya existen, saltando...");
            return;
        }

        var reglas = new List<Regla>
    {
            new Regla
      {
     Clave = "INASISTENCIA_CONSECUTIVA",
                Nombre = "Inasistencia Consecutiva",
     Descripcion = "Detecta participantes con múltiples inasistencias seguidas en una misma actividad",
          Severidad = Severidad.Alta,
                Objetivo = ObjetivoRegla.Participante,
                Activa = true,
  Prioridad = 100,
     Version = 1,
            CreadoEn = DateTime.UtcNow
    },
      new Regla
   {
      Clave = "BAJA_ASISTENCIA_GENERAL",
      Nombre = "Baja Asistencia General",
 Descripcion = "Porcentaje de asistencia por debajo del umbral en el periodo",
   Severidad = Severidad.Alta,
     Objetivo = ObjetivoRegla.Participante,
   Activa = true,
        Prioridad = 90,
       Version = 1,
     CreadoEn = DateTime.UtcNow
 },
       new Regla
       {
       Clave = "ACTIVIDAD_SIN_ASISTENTES",
           Nombre = "Actividad sin asistentes",
      Descripcion = "Actividades planificadas sin inscritos",
       Severidad = Severidad.Info,
       Objetivo = ObjetivoRegla.Actividad,
      Activa = true,
       Prioridad = 50,
      Version = 1,
   CreadoEn = DateTime.UtcNow
  },
         new Regla
      {
  Clave = "RETRASO_ACTIVIDAD",
    Nombre = "Retraso en actividad",
       Descripcion = "Actividad planificadas no ejecutada pasada su fecha",
      Severidad = Severidad.Alta,
       Objetivo = ObjetivoRegla.Actividad,
  Activa = true,
             Prioridad = 80,
    Version = 1,
        CreadoEn = DateTime.UtcNow
            },
         new Regla
            {
       Clave = "BAJO_CUMPLIMIENTO_POA",
        Nombre = "Bajo Cumplimiento de POA",
   Descripcion = "Desvío plan vs ejecución mensual por encima del umbral",
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

        // Parámetros por regla
        var reglaInasistencia = reglas.First(r => r.Clave == "INASISTENCIA_CONSECUTIVA");
        var reglaBajaAsistencia = reglas.First(r => r.Clave == "BAJA_ASISTENCIA_GENERAL");
        var reglaSinAsistentes = reglas.First(r => r.Clave == "ACTIVIDAD_SIN_ASISTENTES");
        var reglaRetraso = reglas.First(r => r.Clave == "RETRASO_ACTIVIDAD");
        var reglaBajoCumplimiento = reglas.First(r => r.Clave == "BAJO_CUMPLIMIENTO_POA");

        var parametros = new List<ReglaParametro>
   {
            new ReglaParametro
          {
  ReglaId = reglaInasistencia.ReglaId,
       Nombre = "UMBRAL_AUSENCIAS",
Tipo = TipoParametro.Entero,
Valor = "3",
 CreadoEn = DateTime.UtcNow
},
            new ReglaParametro
      {
     ReglaId = reglaBajaAsistencia.ReglaId,
      Nombre = "UMBRAL_PCT",
      Tipo = TipoParametro.Decimal,
      Valor = "75",
         CreadoEn = DateTime.UtcNow
},
       new ReglaParametro
  {
          ReglaId = reglaSinAsistentes.ReglaId,
        Nombre = "MIN_INSCRITOS",
    Tipo = TipoParametro.Entero,
     Valor = "1",
    CreadoEn = DateTime.UtcNow
            },
new ReglaParametro
            {
      ReglaId = reglaRetraso.ReglaId,
      Nombre = "DIAS_RETRASO",
   Tipo = TipoParametro.Entero,
  Valor = "3",
                CreadoEn = DateTime.UtcNow
    },
            new ReglaParametro
     {
   ReglaId = reglaBajoCumplimiento.ReglaId,
          Nombre = "UMBRAL_PCT",
         Tipo = TipoParametro.Decimal,
                Valor = "20",
      CreadoEn = DateTime.UtcNow
  }
        };

        await context.ReglaParametros.AddRangeAsync(parametros);
        await context.SaveChangesAsync();

        // Overrides por programa
        var programas = await context.Programas.ToListAsync();
        var progAcademia = programas.First(p => p.Clave == "ACADEMIA");
        var progEDV = programas.First(p => p.Clave == "EDV");

        var overrides = new List<ReglaParametroOverride>
      {
            new ReglaParametroOverride
            {
      ReglaId = reglaBajoCumplimiento.ReglaId,
       ProgramaId = progAcademia.ProgramaId,
          Nombre = "UMBRAL_PCT",
    Tipo = TipoParametro.Decimal,
    Valor = "15",
        CreadoEn = DateTime.UtcNow
            },
     new ReglaParametroOverride
   {
     ReglaId = reglaInasistencia.ReglaId,
           ProgramaId = progEDV.ProgramaId,
             Nombre = "UMBRAL_AUSENCIAS",
    Tipo = TipoParametro.Entero,
                Valor = "2",
       CreadoEn = DateTime.UtcNow
        }
   };

        await context.ReglaParametroOverrides.AddRangeAsync(overrides);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Reglas y Parámetros creados exitosamente");
    }

    private static async Task SeedPersonasYParticipantesAsync(ApplicationDbContext context)
    {
        Console.WriteLine("👥 Seeding Personas y Participantes...");

        if (await context.Personas.CountAsync() > 1)
        {
            Console.WriteLine("⏭️  Personas ya existen, saltando...");
            return;
        }

        // ========== NOMBRES Y APELLIDOS PARA GENERAR PERSONAS ==========
        var nombres = new[] {
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

        var apellidos = new[] {
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

        var random = new Random(42); // Seed fijo para reproducibilidad

        // ========== GENERAR 80 PERSONAS Y PARTICIPANTES ==========
        var personasData = new List<(string Nombres, string Apellidos, DateTime Nac, string Tel, DateTime Alta, string Programa)>();

        // 40 para EDV
        for (int i = 0; i < 40; i++)
        {
            var nombre = nombres[random.Next(nombres.Length)];
            var apellido1 = apellidos[random.Next(apellidos.Length)];
            var apellido2 = apellidos[random.Next(apellidos.Length)];
            var apellido = $"{apellido1} {apellido2}";

            // Edad entre 13 y 25 años
            var edad = random.Next(13, 26);
            var nacimiento = DateTime.Now.AddYears(-edad).AddDays(random.Next(-365, 365));

            var telefono = $"{random.Next(6, 10)}{random.Next(10, 100)}-{random.Next(1000, 10000)}";

            // Fecha de alta entre 6 meses y 2 años atrás
            var alta = DateTime.Now.AddDays(-random.Next(180, 730));

            personasData.Add((nombre, apellido, nacimiento, telefono, alta, "EDV"));
        }

        // 40 para ACADEMIA
        for (int i = 0; i < 40; i++)
        {
            var nombre = nombres[random.Next(nombres.Length)];
            var apellido1 = apellidos[random.Next(apellidos.Length)];
            var apellido2 = apellidos[random.Next(apellidos.Length)];
            var apellido = $"{apellido1} {apellido2}";

            var edad = random.Next(15, 28);
            var nacimiento = DateTime.Now.AddYears(-edad).AddDays(random.Next(-365, 365));

            var telefono = $"{random.Next(6, 10)}{random.Next(10, 100)}-{random.Next(1000, 10000)}";

            var alta = DateTime.Now.AddDays(-random.Next(180, 730));

            personasData.Add((nombre, apellido, nacimiento, telefono, alta, "ACADEMIA"));
        }

        var personas = new List<Persona>();
        var participantes = new List<Participante>();

        foreach (var data in personasData)
        {
            var persona = new Persona
            {
                Nombres = data.Nombres,
                Apellidos = data.Apellidos,
                FechaNacimiento = data.Nac,
                Telefono = data.Tel,
                CreadoEn = DateTime.UtcNow
            };
            personas.Add(persona);
        }

        await context.Personas.AddRangeAsync(personas);
        await context.SaveChangesAsync();

        // Crear participantes
        for (int i = 0; i < personas.Count; i++)
        {
            // 90% activos, 10% inactivos
            var estado = random.Next(100) < 90 ? EstadoGeneral.Activo : EstadoGeneral.Inactivo;

            var participante = new Participante
            {
                PersonaId = personas[i].PersonaId,
                Estado = estado,
                FechaAlta = personasData[i].Alta,
                CreadoEn = DateTime.UtcNow
            };
            participantes.Add(participante);
        }

        await context.Participantes.AddRangeAsync(participantes);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Creadas {personas.Count} personas y {participantes.Count} participantes");
    }

    private static async Task SeedActividadesAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📅 Seeding Actividades...");

        if (await context.Actividades.AnyAsync())
        {
            Console.WriteLine("⏭️  Actividades ya existen, saltando...");
            return;
        }

        var programas = await context.Programas.ToListAsync();
        var progEDV = programas.First(p => p.Clave == "EDV");
        var progACADEMIA = programas.First(p => p.Clave == "ACADEMIA");
        var progJuventudSegura = programas.First(p => p.Clave == "JUVENTUD_SEGURA");
        var progBernabe = programas.First(p => p.Clave == "BERNABE");

        var actividades = new List<Actividad>();
        var random = new Random(42);

        var titulosEDV = new[] {
            "Taller de Valores", "Formación en Liderazgo", "Convivencia Escolar",
            "Prevención de Violencia", "Cultura de Paz", "Resolución de Conflictos",
            "Comunicación Asertiva", "Trabajo en Equipo", "Empatía y Respeto"
        };

        var titulosAcademia = new[] {
            "Mentoría Individual", "Coaching Grupal", "Desarrollo Personal",
            "Planificación de Vida", "Habilidades Blandas", "Oratoria",
            "Gestión del Tiempo", "Inteligencia Emocional", "Liderazgo Juvenil"
        };

        var lugares = new[] { "Salón A", "Salón B", "Auditorio", "Aula 1", "Aula 2", "Patio Central", "Biblioteca" };

        // ========== EDV: 60 actividades en los últimos 6 meses ==========
        var fechaInicio = DateTime.Now.AddMonths(-6);
        for (int i = 0; i < 60; i++)
        {
            // Actividades semanales (miércoles y viernes)
            var diasDesdeinicio = i * 3; // Aproximadamente 2 por semana
            var fechaActividad = fechaInicio.AddDays(diasDesdeinicio);

            // Ajustar al miércoles o viernes más cercano
            while (fechaActividad.DayOfWeek != DayOfWeek.Wednesday && fechaActividad.DayOfWeek != DayOfWeek.Friday)
            {
                fechaActividad = fechaActividad.AddDays(1);
            }

            var titulo = titulosEDV[random.Next(titulosEDV.Length)] + $" {i + 1:000}";
            
            // ESCENARIO 5: Crear actividades RETRASADAS (planificadas pero no ejecutadas después de 7+ días) para alertas ALTA
            EstadoActividad estado;
            if (fechaActividad < DateTime.Now.AddDays(-10) && i % 8 == 0) // Cada 8va actividad antigua
            {
                estado = EstadoActividad.Planificada; // RETRASO_ACTIVIDAD -> ALTA
                Console.WriteLine($"🎯 Actividad RETRASADA: {titulo} - Fecha: {fechaActividad:dd/MM/yyyy} (alerta ALTA)");
            }
            else
            {
                estado = fechaActividad < DateTime.Now.AddDays(-7)
                    ? (random.Next(100) < 85 ? EstadoActividad.Realizada : EstadoActividad.Planificada)
                    : EstadoActividad.Planificada;
            }

            actividades.Add(new Actividad
            {
                ProgramaId = progEDV.ProgramaId,
                Titulo = titulo,
                Descripcion = $"Sesión de formación en valores para jóvenes - {titulo}",
                FechaInicio = fechaActividad.Date.AddHours(18),
                FechaFin = fechaActividad.Date.AddHours(20),
                Lugar = lugares[random.Next(lugares.Length)],
                Tipo = random.Next(100) < 70 ? TipoActividad.Taller : (random.Next(100) < 50 ? TipoActividad.Capacitacion : TipoActividad.Evento),
                Estado = estado,
                CreadoEn = DateTime.UtcNow
            });
        }

        // ========== ACADEMIA: 40 actividades en los últimos 6 meses ==========
        for (int i = 0; i < 40; i++)
        {
            var diasDesdeInicio = i * 4; // Aproximadamente 1-2 por semana
            var fechaActividad = fechaInicio.AddDays(diasDesdeInicio);

            var titulo = titulosAcademia[random.Next(titulosAcademia.Length)] + $" {i + 1:000}";
            
            // ESCENARIO 6: Actividades retrasadas en ACADEMIA
            EstadoActividad estado;
            if (fechaActividad < DateTime.Now.AddDays(-10) && i % 10 == 0) // Cada 10ma actividad antigua
            {
                estado = EstadoActividad.Planificada; // RETRASO_ACTIVIDAD -> ALTA
                Console.WriteLine($"🎯 Actividad RETRASADA: {titulo} - Fecha: {fechaActividad:dd/MM/yyyy} (alerta ALTA)");
            }
            else
            {
                estado = fechaActividad < DateTime.Now.AddDays(-7)
                    ? (random.Next(100) < 80 ? EstadoActividad.Realizada : EstadoActividad.Planificada)
                    : EstadoActividad.Planificada;
            }

            actividades.Add(new Actividad
            {
                ProgramaId = progACADEMIA.ProgramaId,
                Titulo = titulo,
                Descripcion = $"Desarrollo de habilidades de liderazgo - {titulo}",
                FechaInicio = fechaActividad.Date.AddHours(17),
                FechaFin = fechaActividad.Date.AddHours(19),
                Lugar = lugares[random.Next(lugares.Length)],
                Tipo = random.Next(100) < 60 ? TipoActividad.Taller : TipoActividad.Reunion,
                Estado = estado,
                CreadoEn = DateTime.UtcNow
            });
        }

        // ========== JUVENTUD SEGURA: 30 actividades ==========
        for (int i = 0; i < 30; i++)
        {
            var diasDesdeInicio = i * 6;
            var fechaActividad = fechaInicio.AddDays(diasDesdeInicio);

            var estado = fechaActividad < DateTime.Now.AddDays(-7)
                ? (random.Next(100) < 75 ? EstadoActividad.Realizada : EstadoActividad.Planificada)
                : EstadoActividad.Planificada;

            actividades.Add(new Actividad
            {
                ProgramaId = progJuventudSegura.ProgramaId,
                Titulo = $"Prevención y Seguridad {i + 1:000}",
                Descripcion = "Actividad de prevención y seguridad para jóvenes",
                FechaInicio = fechaActividad.Date.AddHours(16),
                FechaFin = fechaActividad.Date.AddHours(18),
                Lugar = lugares[random.Next(lugares.Length)],
                Tipo = TipoActividad.Capacitacion,
                Estado = estado,
                CreadoEn = DateTime.UtcNow
            });
        }

        // ========== BERNABÉ: 20 actividades ==========
        for (int i = 0; i < 20; i++)
        {
            var diasDesdeInicio = i * 9;
            var fechaActividad = fechaInicio.AddDays(diasDesdeInicio);

            var estado = fechaActividad < DateTime.Now.AddDays(-7)
                ? (random.Next(100) < 70 ? EstadoActividad.Realizada : EstadoActividad.Planificada)
                : EstadoActividad.Planificada;

            actividades.Add(new Actividad
            {
                ProgramaId = progBernabe.ProgramaId,
                Titulo = $"Acompañamiento Integral {i + 1:000}",
                Descripcion = "Sesión de acompañamiento integral a jóvenes",
                FechaInicio = fechaActividad.Date.AddHours(15),
                FechaFin = fechaActividad.Date.AddHours(17),
                Lugar = lugares[random.Next(lugares.Length)],
                Tipo = TipoActividad.Reunion,
                Estado = estado,
                CreadoEn = DateTime.UtcNow
            });
        }

        await context.Actividades.AddRangeAsync(actividades);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Creadas {actividades.Count} actividades");
        Console.WriteLine($"🎯 Actividades retrasadas para alertas ALTA: ~{60 / 8 + 40 / 10}");
    }

    private static async Task SeedActividadParticipantesAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📝 Seeding Inscripciones a Actividades...");

        if (await context.ActividadParticipantes.AnyAsync())
        {
            Console.WriteLine("⏭️  Inscripciones ya existen, saltando...");
            return;
        }

        var actividades = await context.Actividades.Include(a => a.Programa).ToListAsync();
        var participantes = await context.Participantes.Include(p => p.Persona).ToListAsync();
        var random = new Random(42);

        // Dividir participantes por programa (primeros 40 EDV, siguientes 40 ACADEMIA)
        var partEDV = participantes.Take(40).ToList();
        var partACADEMIA = participantes.Skip(40).Take(40).ToList();

        var inscripciones = new List<ActividadParticipante>();

        // ========== EDV: Inscribir 25-35 participantes por actividad ==========
        var actividadesEDV = actividades.Where(a => a.Programa.Clave == "EDV").OrderBy(a => a.FechaInicio).ToList();
        
        // ESCENARIO 1: Crear 2-3 actividades SIN INSCRIPTOS para generar alertas INFO
        for (int i = 0; i < Math.Min(3, actividadesEDV.Count); i++)
        {
            // Las primeras 3 actividades EDV no tendrán participantes (ACTIVIDAD_SIN_ASISTENTES -> INFO)
            Console.WriteLine($"🎯 Actividad {actividadesEDV[i].Titulo} - SIN INSCRIPTOS (generará alerta INFO)");
        }
        
        // Resto de actividades EDV con participantes normales
        for (int i = 3; i < actividadesEDV.Count; i++)
        {
            var actividad = actividadesEDV[i];
            var cantidadParticipantes = random.Next(25, 36);
            var participantesSeleccionados = partEDV.OrderBy(_ => random.Next()).Take(cantidadParticipantes);

            foreach (var participante in participantesSeleccionados)
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

        // ========== ACADEMIA: Inscribir 15-25 participantes por actividad ==========
        var actividadesACADEMIA = actividades.Where(a => a.Programa.Clave == "ACADEMIA").OrderBy(a => a.FechaInicio).ToList();
        
        // ESCENARIO 2: Crear 2 actividades SIN INSCRIPTOS para generar alertas INFO
        for (int i = 0; i < Math.Min(2, actividadesACADEMIA.Count); i++)
        {
            Console.WriteLine($"🎯 Actividad {actividadesACADEMIA[i].Titulo} - SIN INSCRIPTOS (generará alerta INFO)");
        }
        
        // Resto con participantes normales
        for (int i = 2; i < actividadesACADEMIA.Count; i++)
        {
            var actividad = actividadesACADEMIA[i];
            var cantidadParticipantes = random.Next(15, 26);
            var participantesSeleccionados = partACADEMIA.OrderBy(_ => random.Next()).Take(cantidadParticipantes);

            foreach (var participante in participantesSeleccionados)
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

        // ========== OTROS PROGRAMAS: Inscribir participantes variados ==========
        var actividadesOtros = actividades.Where(a => a.Programa.Clave != "EDV" && a.Programa.Clave != "ACADEMIA").ToList();
        foreach (var actividad in actividadesOtros)
        {
            var cantidadParticipantes = random.Next(10, 21);
            var todosParticipantes = partEDV.Concat(partACADEMIA).ToList();
            var participantesSeleccionados = todosParticipantes.OrderBy(_ => random.Next()).Take(cantidadParticipantes);

            foreach (var participante in participantesSeleccionados)
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

        Console.WriteLine($"✅ Creadas {inscripciones.Count} inscripciones a actividades");
        Console.WriteLine($"🎯 Escenarios creados: {3 + 2} actividades sin inscriptos (alertas INFO)");
    }

    private static async Task SeedAsistenciasAsync(ApplicationDbContext context)
    {
        Console.WriteLine("✅ Seeding Asistencias...");

        if (await context.Asistencias.AnyAsync())
        {
            Console.WriteLine("⏭️  Asistencias ya existen, saltando...");
            return;
        }

        var inscripciones = await context.ActividadParticipantes
            .Include(ap => ap.Actividad)
            .Include(ap => ap.Participante)
            .Where(ap => ap.Actividad.Estado == EstadoActividad.Realizada && ap.Estado == EstadoInscripcion.Inscrito)
            .OrderBy(ap => ap.Actividad.FechaInicio)
            .ThenBy(ap => ap.ParticipanteId)
            .ToListAsync();

        var asistencias = new List<Asistencia>();
        var random = new Random(42);

        var observaciones = new[] {
            "Participó activamente", "Llegó tarde", "Se retiró temprano",
            "Excelente participación", "Mostró interés", "Poca participación",
            null, null, null, null // 40% sin observación
        };

        // ESCENARIO 3: Crear participantes con BAJA ASISTENCIA GENERAL (< 75%) para alertas ALTA
        // Seleccionar 5 participantes problema
        var participantesProblema = inscripciones
            .GroupBy(i => i.ParticipanteId)
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        Console.WriteLine($"🎯 Participantes con baja asistencia programados: {participantesProblema.Count} (alertas ALTA)");

        // ESCENARIO 4: Crear participantes con INASISTENCIAS CONSECUTIVAS (3+ ausencias seguidas) para alertas ALTA
        // Seleccionar 3 participantes con inasistencias consecutivas
        var participantesInasistenciaConsecutiva = inscripciones
            .GroupBy(i => i.ParticipanteId)
            .Skip(5)
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        Console.WriteLine($"🎯 Participantes con inasistencias consecutivas programados: {participantesInasistenciaConsecutiva.Count} (alertas ALTA)");

        foreach (var inscripcion in inscripciones)
        {
            EstadoAsistencia estado;

            // ESCENARIO: Participantes con baja asistencia general (60% ausente)
            if (participantesProblema.Contains(inscripcion.ParticipanteId))
            {
                var dado = random.Next(100);
                if (dado < 40)
                    estado = EstadoAsistencia.Presente; // Solo 40% presente
                else if (dado < 60)
                    estado = EstadoAsistencia.Tarde;
                else
                    estado = EstadoAsistencia.Ausente; // 40% ausente
            }
            // ESCENARIO: Participantes con inasistencias consecutivas
            else if (participantesInasistenciaConsecutiva.Contains(inscripcion.ParticipanteId))
            {
                // Crear patrón: P, P, A, A, A, A, P, P (4 ausencias consecutivas)
                var actividadesDelParticipante = inscripciones
                    .Where(i => i.ParticipanteId == inscripcion.ParticipanteId)
                    .OrderBy(i => i.Actividad.FechaInicio)
                    .ToList();
                
                var indice = actividadesDelParticipante.IndexOf(inscripcion);
                
                // Ausente en actividades 2, 3, 4, 5 (índices 2-5)
                if (indice >= 2 && indice <= 5)
                    estado = EstadoAsistencia.Ausente;
                else
                    estado = EstadoAsistencia.Presente;
            }
            // Resto: distribución normal (82% presentes)
            else
            {
                var dado = random.Next(100);
                if (dado < 82)
                    estado = EstadoAsistencia.Presente;
                else if (dado < 92)
                    estado = EstadoAsistencia.Ausente;
                else if (dado < 97)
                    estado = EstadoAsistencia.Tarde;
                else
                    estado = EstadoAsistencia.Justificado;
            }

            asistencias.Add(new Asistencia
            {
                ActividadId = inscripcion.ActividadId,
                ParticipanteId = inscripcion.ParticipanteId,
                Fecha = inscripcion.Actividad.FechaInicio.Date,
                Estado = estado,
                Observacion = random.Next(100) < 30 ? observaciones[random.Next(observaciones.Length)] : null,
                CreadoEn = DateTime.UtcNow
            });
        }

        await context.Asistencias.AddRangeAsync(asistencias);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Creadas {asistencias.Count} asistencias");
        Console.WriteLine($"🎯 Escenarios para motor de inferencia:");
        Console.WriteLine($"   - {participantesProblema.Count} participantes con baja asistencia general < 75% (ALTA)");
        Console.WriteLine($"   - {participantesInasistenciaConsecutiva.Count} participantes con 3+ inasistencias consecutivas (ALTA)");
    }

    private static async Task SeedEvidenciasAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📎 Seeding Evidencias de Actividades...");

        if (await context.EvidenciaActividades.AnyAsync())
        {
            Console.WriteLine("⏭️  Evidencias ya existen, saltando...");
            return;
        }

        var actividades = await context.Actividades
            .Where(a => a.Estado == EstadoActividad.Realizada && !a.IsDeleted)
            .ToListAsync();

        var evidencias = new List<EvidenciaActividad>();
        var random = new Random(42);

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
                // 1-3 evidencias por actividad
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

        Console.WriteLine($"✅ Creadas {evidencias.Count} evidencias para {actividades.Count} actividades");
    }

    private static async Task SeedMetricasProgramaMesAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📊 Seeding Métricas Mensuales...");

        if (await context.MetricasProgramaMes.AnyAsync())
        {
            Console.WriteLine("⏭️  Métricas ya existen, saltando...");
            return;
        }

        var programas = await context.Programas.ToListAsync();
        var metricas = new List<MetricasProgramaMes>();
        var random = new Random(42);

        // Generar métricas para los últimos 6 meses
        var fechaInicio = DateTime.Now.AddMonths(-6);

        for (int mes = 0; mes < 6; mes++)
        {
            var fecha = fechaInicio.AddMonths(mes);
            var anioMes = $"{fecha.Year:0000}-{fecha.Month:00}";

            foreach (var programa in programas)
            {
                // Calcular métricas reales
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
                var presentes = asistencias.Count(a => a.Estado == EstadoAsistencia.Presente || a.Estado == EstadoAsistencia.Tarde);
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

        Console.WriteLine($"✅ Creadas {metricas.Count} métricas mensuales");
    }

    private static async Task SeedPOADinamicoAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📋 Seeding POA Dinámico (opcional)...");

        if (await context.POAPlantillas.AnyAsync())
        {
            Console.WriteLine("⏭️  POA ya existe, saltando...");
            return;
        }

        var programas = await context.Programas.ToListAsync();
        var progEDV = programas.First(p => p.Clave == "EDV");
        var progACADEMIA = programas.First(p => p.Clave == "ACADEMIA");

        // Plantillas
        var plantillaEDV = new POAPlantilla
        {
            ProgramaId = progEDV.ProgramaId,
            Version = 1,
            Estado = EstadoPlantilla.Activa,
            VigenteDesde = new DateTime(2025, 10, 1),
            CreadoEn = DateTime.UtcNow
        };

        var plantillaACADEMIA = new POAPlantilla
        {
            ProgramaId = progACADEMIA.ProgramaId,
            Version = 1,
            Estado = EstadoPlantilla.Activa,
            VigenteDesde = new DateTime(2025, 10, 1),
            CreadoEn = DateTime.UtcNow
        };

        await context.POAPlantillas.AddRangeAsync(new[] { plantillaEDV, plantillaACADEMIA });
        await context.SaveChangesAsync();

        // ========== CAMPOS COMPLETOS PARA EDV ==========
        var camposEDV = new List<POACampo>
        {
            // 1. Actividades
            new POACampo
            {
                PlantillaId = plantillaEDV.PlantillaId,
                Clave = "ACTIVIDADES_PLANIFICADAS",
                Etiqueta = "Actividades Planificadas",
                TipoDato = TipoDato.Entero,
                Requerido = true,
                Orden = 1,
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            new POACampo
            {
                PlantillaId = plantillaEDV.PlantillaId,
                Clave = "ACTIVIDADES_EJECUTADAS",
                Etiqueta = "Actividades Ejecutadas",
                TipoDato = TipoDato.Entero,
                Requerido = false,
                Orden = 2,
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            
            // 2. Presupuesto
            new POACampo
            {
                PlantillaId = plantillaEDV.PlantillaId,
                Clave = "PRESUPUESTO_TOTAL",
                Etiqueta = "Presupuesto Total",
                TipoDato = TipoDato.Decimal,
                Requerido = false,
                Orden = 3,
                Unidad = "USD",
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            new POACampo
            {
                PlantillaId = plantillaEDV.PlantillaId,
                Clave = "PRESUPUESTO_EJECUTADO",
                Etiqueta = "Presupuesto Ejecutado",
                TipoDato = TipoDato.Decimal,
                Requerido = false,
                Orden = 4,
                Unidad = "USD",
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            
            // 3. Participantes
            new POACampo
            {
                PlantillaId = plantillaEDV.PlantillaId,
                Clave = "TOTAL_PARTICIPANTES",
                Etiqueta = "Total Participantes",
                TipoDato = TipoDato.Entero,
                Requerido = false,
                Orden = 5,
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            new POACampo
            {
                PlantillaId = plantillaEDV.PlantillaId,
                Clave = "PARTICIPANTES_ACTIVOS",
                Etiqueta = "Participantes Activos",
                TipoDato = TipoDato.Entero,
                Requerido = false,
                Orden = 6,
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },

           new POACampo
            {
                PlantillaId = plantillaEDV.PlantillaId,
                Clave = "PORCENTAJE_ASISTENCIA",
                Etiqueta = "Porcentaje de Asistencia Promedio",
                TipoDato = TipoDato.Decimal,
                Requerido = false,
                Orden = 7,
                Unidad = "%",
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            new POACampo
            {
                PlantillaId = plantillaEDV.PlantillaId,
                Clave = "PORCENTAJE_CUMPLIMIENTO",
                Etiqueta = "Porcentaje de Cumplimiento General",
                TipoDato = TipoDato.Decimal,
                Requerido = false,
                Orden = 8,
                Unidad = "%",
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            }
        };

        // ========== CAMPOS COMPLETOS PARA ACADEMIA ==========
        var camposACADEMIA = new List<POACampo>
        {
            // 1. Actividades
            new POACampo
            {
                PlantillaId = plantillaACADEMIA.PlantillaId,
                Clave = "ACTIVIDADES_PLANIFICADAS",
                Etiqueta = "Sesiones Planificadas",
                TipoDato = TipoDato.Entero,
                Requerido = true,
                Orden = 1,
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            new POACampo
            {
                PlantillaId = plantillaACADEMIA.PlantillaId,
                Clave = "ACTIVIDADES_EJECUTADAS",
                Etiqueta = "Sesiones Ejecutadas",
                TipoDato = TipoDato.Entero,
                Requerido = false,
                Orden = 2,
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            
            // 2. Presupuesto
            new POACampo
            {
                PlantillaId = plantillaACADEMIA.PlantillaId,
                Clave = "PRESUPUESTO_TOTAL",
                Etiqueta = "Presupuesto Total",
                TipoDato = TipoDato.Decimal,
                Requerido = false,
                Orden = 3,
                Unidad = "USD",
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            new POACampo
            {
                PlantillaId = plantillaACADEMIA.PlantillaId,
                Clave = "PRESUPUESTO_EJECUTADO",
                Etiqueta = "Presupuesto Ejecutado",
                TipoDato = TipoDato.Decimal,
                Requerido = false,
                Orden = 4,
                Unidad = "USD",
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            
            // 3. Participantes
            new POACampo
            {
                PlantillaId = plantillaACADEMIA.PlantillaId,
                Clave = "TOTAL_PARTICIPANTES",
                Etiqueta = "Total Participantes",
                TipoDato = TipoDato.Entero,
                Requerido = false,
                Orden = 5,
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            new POACampo
            {
                PlantillaId = plantillaACADEMIA.PlantillaId,
                Clave = "PARTICIPANTES_ACTIVOS",
                Etiqueta = "Participantes Activos",
                TipoDato = TipoDato.Entero,
                Requerido = false,
                Orden = 6,
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },

           new POACampo
            {
                PlantillaId = plantillaACADEMIA.PlantillaId,
                Clave = "PORCENTAJE_ASISTENCIA",
                Etiqueta = "Porcentaje de Asistencia Promedio",
                TipoDato = TipoDato.Decimal,
                Requerido = false,
                Orden = 7,
                Unidad = "%",
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            },
            new POACampo
            {
                PlantillaId = plantillaACADEMIA.PlantillaId,
                Clave = "PORCENTAJE_CUMPLIMIENTO",
                Etiqueta = "Porcentaje de Cumplimiento General",
                TipoDato = TipoDato.Decimal,
                Requerido = false,
                Orden = 8,
                Unidad = "%",
                Alcance = AlcancePOA.Programa,
                CreadoEn = DateTime.UtcNow
            }
        };

        await context.POACampos.AddRangeAsync(camposEDV);
        await context.POACampos.AddRangeAsync(camposACADEMIA);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Campos POA creados exitosamente");

        // Instancias octubre 2025
        var instanciaEDV = new POAInstancia
        {
            ProgramaId = progEDV.ProgramaId,
            PlantillaId = plantillaEDV.PlantillaId,
            PeriodoAnio = 2025,
            PeriodoMes = 10,
            Estado = EstadoInstancia.Aprobado,
            CreadoEn = DateTime.UtcNow
        };

        var instanciaACADEMIA = new POAInstancia
        {
            ProgramaId = progACADEMIA.ProgramaId,
            PlantillaId = plantillaACADEMIA.PlantillaId,
            PeriodoAnio = 2025,
            PeriodoMes = 10,
            Estado = EstadoInstancia.Aprobado,
            CreadoEn = DateTime.UtcNow
        };

        await context.POAInstancias.AddRangeAsync(new[] { instanciaEDV, instanciaACADEMIA });
        await context.SaveChangesAsync();

        // Calcular valores reales desde las actividades
        var actividadesEDV = await context.Actividades
            .Where(a => a.ProgramaId == progEDV.ProgramaId && 
                       a.FechaInicio.Year == 2025 && 
                       a.FechaInicio.Month == 10 &&
                       !a.IsDeleted)
            .ToListAsync();

        var actividadesACADEMIA = await context.Actividades
            .Where(a => a.ProgramaId == progACADEMIA.ProgramaId && 
                       a.FechaInicio.Year == 2025 && 
                       a.FechaInicio.Month == 10 &&
                       !a.IsDeleted)
            .ToListAsync();

        // ESCENARIO 7: Generar BAJO CUMPLIMIENTO POA para alertas CRÍTICAS
        // EDV: Planificadas 20, pero solo ejecutadas 10 = 50% (umbral 20% = alerta crítica)
        var planificadasEDV = 20;
        var ejecutadasEDV = 10; // 50% cumplimiento -> desviación 50% > 20% umbral
        
        // ACADEMIA: Planificadas 15, pero solo ejecutadas 11 = 73.3% (umbral 15% = alerta crítica)
        var planificadasACADEMIA = 15;
        var ejecutadasACADEMIA = 11; // 73.3% cumplimiento -> desviación 26.7% > 15% umbral

        Console.WriteLine($"🎯 POA EDV: Planificadas {planificadasEDV}, Ejecutadas {ejecutadasEDV} = {(ejecutadasEDV * 100.0 / planificadasEDV):F1}% (alerta CRÍTICA esperada)");
        Console.WriteLine($"🎯 POA ACADEMIA: Planificadas {planificadasACADEMIA}, Ejecutadas {ejecutadasACADEMIA} = {(ejecutadasACADEMIA * 100.0 / planificadasACADEMIA):F1}% (alerta CRÍTICA esperada)");

        var valores = new List<POAValor>
        {
            // EDV - Valores que generan alerta crítica
            new POAValor
            {
                InstanciaId = instanciaEDV.InstanciaId,
                CampoId = camposEDV[0].CampoId,
                ProgramaId = progEDV.ProgramaId,
                ValorNumero = planificadasEDV,
                ValorDecimal = planificadasEDV,
                CreadoEn = DateTime.UtcNow
            },
            new POAValor
            {
                InstanciaId = instanciaEDV.InstanciaId,
                CampoId = camposEDV[1].CampoId,
                ProgramaId = progEDV.ProgramaId,
                ValorNumero = ejecutadasEDV,
                ValorDecimal = ejecutadasEDV,
                CreadoEn = DateTime.UtcNow
            },
            
            // ACADEMIA - Valores que generan alerta crítica
            new POAValor
            {
                InstanciaId = instanciaACADEMIA.InstanciaId,
                CampoId = camposACADEMIA[0].CampoId,
                ProgramaId = progACADEMIA.ProgramaId,
                ValorNumero = planificadasACADEMIA,
                ValorDecimal = planificadasACADEMIA,
                CreadoEn = DateTime.UtcNow
            },
            new POAValor
            {
                InstanciaId = instanciaACADEMIA.InstanciaId,
                CampoId = camposACADEMIA[1].CampoId,
                ProgramaId = progACADEMIA.ProgramaId,
                ValorNumero = ejecutadasACADEMIA,
                ValorDecimal = ejecutadasACADEMIA,
                CreadoEn = DateTime.UtcNow
            }
        };

        await context.POAValores.AddRangeAsync(valores);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ POA Dinámico creado exitosamente con campos completos");
    }

    private static async Task SeedDiccionarioObservacionesAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📖 Seeding Diccionario de Observaciones (opcional)...");

        if (await context.DiccionarioObservaciones.AnyAsync())
        {
            Console.WriteLine("⏭️  Diccionario ya existe, saltando...");
            return;
        }

        var programas = await context.Programas.ToListAsync();
        var progEDV = programas.First(p => p.Clave == "EDV");
        var progACADEMIA = programas.First(p => p.Clave == "ACADEMIA");

        var diccionarios = new List<DiccionarioObservaciones>
        {
            new DiccionarioObservaciones
            {
                Expresion = "llegó tarde",
                Ponderacion = 0.2m,
                Ambito = AmbitoDiccionario.Global,
                Activo = true,
                CreadoEn = DateTime.UtcNow
            },
            new DiccionarioObservaciones
            {
                Expresion = "no asistió",
                Ponderacion = 0.5m,
                Ambito = AmbitoDiccionario.Global,
                Activo = true,
                CreadoEn = DateTime.UtcNow
            }
        };

        await context.DiccionarioObservaciones.AddRangeAsync(diccionarios);
        await context.SaveChangesAsync();

        // Relación con programas
        var relaciones = new List<DiccionarioObservacionesPrograma>();
        foreach (var dic in diccionarios)
        {
            if (dic.Expresion == "llegó tarde")
            {
                relaciones.Add(new DiccionarioObservacionesPrograma
                {
                    DiccionarioId = dic.DiccionarioId,
                    ProgramaId = progEDV.ProgramaId,
                    CreadoEn = DateTime.UtcNow
                });
                relaciones.Add(new DiccionarioObservacionesPrograma
                {
                    DiccionarioId = dic.DiccionarioId,
                    ProgramaId = progACADEMIA.ProgramaId,
                    CreadoEn = DateTime.UtcNow
                });
            }
            else if (dic.Expresion == "no asistió")
            {
                relaciones.Add(new DiccionarioObservacionesPrograma
                {
                    DiccionarioId = dic.DiccionarioId,
                    ProgramaId = progEDV.ProgramaId,
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        await context.DiccionarioObservacionesProgramas.AddRangeAsync(relaciones);
        await context.SaveChangesAsync();

        Console.WriteLine("Diccionario de Observaciones creado exitosamente");
    }
}

