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

        // ========== 10) MÉTRICAS MENSUALES ==========
        await SeedMetricasProgramaMesAsync(context);

        // ========== 11) POA DINÁMICO (OPCIONAL) ==========
        await SeedPOADinamicoAsync(context);

        // ========== 12) DICCIONARIO DE OBSERVACIONES (OPCIONAL) ==========
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
        Console.WriteLine("👤 Seeding Usuario Administrador...");

        if (await userManager.Users.AnyAsync())
        {
            Console.WriteLine("⏭️  Usuarios ya existen, saltando...");
            return;
        }

        // Crear persona
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

        // Crear usuario
        var adminUser = new Usuario
        {
            PersonaId = personaAdmin.PersonaId,
            UserName = "admin@ong.com",
            Email = "admin@ong.com",
            EmailConfirmed = true,
            Estado = EstadoGeneral.Activo,
            MustChangePassword = false, // Admin no necesita cambiar password en primer login
            CreatedBy = "Sistema",
            CreatedAtUtc = DateTime.UtcNow,
            CreadoEn = DateTime.UtcNow
        };

        // Nueva contraseña: 12+ caracteres, mayúsculas, minúsculas, números y caracteres especiales
        var result = await userManager.CreateAsync(adminUser, "Administrador@2025!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrador");

            // Asignar a programas EDV y ACADEMIA
            var programas = await context.Programas.Where(p => p.Clave == "EDV" || p.Clave == "ACADEMIA").ToListAsync();
            foreach (var prog in programas)
            {
                context.UsuarioProgramas.Add(new UsuarioPrograma
                {
                    UsuarioId = adminUser.Id,
                    ProgramaId = prog.ProgramaId,
                    Desde = new DateTime(2025, 10, 1),
                    CreadoEn = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Usuario administrador creado (admin@ong.com / Admin@2025!)");
        }
        else
        {
            Console.WriteLine($"❌ Error creando admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
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
       Descripcion = "Actividad planificada no ejecutada pasada su fecha",
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

        if (await context.Personas.CountAsync() > 1) // Ya hay más que solo el admin
        {
            Console.WriteLine("⏭️  Personas ya existen, saltando...");
            return;
        }

        var personasData = new List<(string Nombres, string Apellidos, DateTime Nac, string Tel, DateTime Alta)>
  {
      // EDV
            ("Ana Sofía", "Rojas Pérez", new DateTime(2008, 5, 12), "711-0001", new DateTime(2025, 9, 1)),
      ("Luis Alberto", "Quiroga Mendoza", new DateTime(2007, 3, 8), "711-0002", new DateTime(2025, 9, 1)),
            ("Camila", "Flores Vargas", new DateTime(2006, 11, 25), "711-0003", new DateTime(2025, 9, 1)),
          // ACADEMIA
 ("Diego", "Fernández Ortiz", new DateTime(2005, 2, 14), "722-0001", new DateTime(2025, 9, 1)),
     ("Mariana", "Gutiérrez Salas", new DateTime(2006, 7, 19), "722-0002", new DateTime(2025, 9, 1)),
  ("Javier", "Salazar Pinto", new DateTime(2004, 9, 30), "722-0003", new DateTime(2025, 9, 1))
    };

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
            var participante = new Participante
            {
                PersonaId = personas[i].PersonaId,
                Estado = EstadoGeneral.Activo,
                FechaAlta = personasData[i].Alta,
                CreadoEn = DateTime.UtcNow
            };
            participantes.Add(participante);
        }

        await context.Participantes.AddRangeAsync(participantes);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Personas y Participantes creados exitosamente");
    }

    private static async Task SeedActividadesAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📅 Seeding Actividades octubre 2025...");

        if (await context.Actividades.AnyAsync())
        {
            Console.WriteLine("⏭️  Actividades ya existen, saltando...");
            return;
        }

        var programas = await context.Programas.ToListAsync();
        var progEDV = programas.First(p => p.Clave == "EDV");
        var progACADEMIA = programas.First(p => p.Clave == "ACADEMIA");

        var actividades = new List<Actividad>
  {
            // EDV - 3 sesiones semanales (miércoles)
  new Actividad
     {
      ProgramaId = progEDV.ProgramaId,
           Titulo = "EDV Taller 101",
      Descripcion = "Primera sesión de valores",
     FechaInicio = new DateTime(2025, 10, 1, 18, 0, 0),
      FechaFin = new DateTime(2025, 10, 1, 20, 0, 0),
          Lugar = "Salón A",
                Tipo = TipoActividad.Taller,
       Estado = EstadoActividad.Planificada,
     CreadoEn = DateTime.UtcNow
            },
new Actividad
   {
 ProgramaId = progEDV.ProgramaId,
    Titulo = "EDV Taller 102",
    Descripcion = "Segunda sesión de valores",
                FechaInicio = new DateTime(2025, 10, 8, 18, 0, 0),
                FechaFin = new DateTime(2025, 10, 8, 20, 0, 0),
Lugar = "Salón A",
Tipo = TipoActividad.Taller,
        Estado = EstadoActividad.Planificada,
 CreadoEn = DateTime.UtcNow
 },
            new Actividad
      {
      ProgramaId = progEDV.ProgramaId,
    Titulo = "EDV Taller 103",
              Descripcion = "Tercera sesión de valores",
   FechaInicio = new DateTime(2025, 10, 15, 18, 0, 0),
            FechaFin = new DateTime(2025, 10, 15, 20, 0, 0),
   Lugar = "Salón A",
Tipo = TipoActividad.Taller,
           Estado = EstadoActividad.Planificada,
       CreadoEn = DateTime.UtcNow
            },
            // ACADEMIA
  new Actividad
    {
     ProgramaId = progACADEMIA.ProgramaId,
         Titulo = "Mentoría Liderazgo",
    Descripcion = "Sesión de mentoría - Para RETRASO_ACTIVIDAD",
   FechaInicio = new DateTime(2025, 10, 5, 17, 0, 0),
             FechaFin = new DateTime(2025, 10, 5, 19, 0, 0),
           Lugar = "Auditorio",
           Tipo = TipoActividad.Taller,
     Estado = EstadoActividad.Planificada, // No se ejecutó
        CreadoEn = DateTime.UtcNow
            },
   new Actividad
        {
            ProgramaId = progACADEMIA.ProgramaId,
    Titulo = "Sesión Debate",
         Descripcion = "Debate de liderazgo - Para ACTIVIDAD_SIN_ASISTENTES",
        FechaInicio = new DateTime(2025, 10, 20, 17, 0, 0),
      FechaFin = new DateTime(2025, 10, 20, 19, 0, 0),
  Lugar = "Aula 2",
                Tipo = TipoActividad.Taller,
       Estado = EstadoActividad.Planificada,
        CreadoEn = DateTime.UtcNow
            }
        };

        await context.Actividades.AddRangeAsync(actividades);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Actividades creadas exitosamente");
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

        // EDV: participantes 1, 2, 3
        var partEDV = participantes.Take(3).ToList();
        // ACADEMIA: participantes 4, 5
        var partACADEMIA = participantes.Skip(3).Take(2).ToList();

        var inscripciones = new List<ActividadParticipante>();

        // EDV-ACT-01, 02, 03 -> inscribir a P-EDV-01, 02, 03
        var actividadesEDV = actividades.Where(a => a.Programa.Clave == "EDV").ToList();
        foreach (var act in actividadesEDV)
        {
            foreach (var part in partEDV)
            {
                inscripciones.Add(new ActividadParticipante
                {
                    ActividadId = act.ActividadId,
                    ParticipanteId = part.ParticipanteId,
                    Rol = RolParticipante.Asistente,
                    Estado = EstadoInscripcion.Inscrito,
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        // ACA-ACT-01 -> inscribir a P-ACA-01 y P-ACA-02
        var actMentoria = actividades.First(a => a.Titulo == "Mentoría Liderazgo");
        foreach (var part in partACADEMIA)
        {
            inscripciones.Add(new ActividadParticipante
            {
                ActividadId = actMentoria.ActividadId,
                ParticipanteId = part.ParticipanteId,
                Rol = RolParticipante.Asistente,
                Estado = EstadoInscripcion.Inscrito,
                CreadoEn = DateTime.UtcNow
            });
        }

        // ACA-ACT-02 -> NO inscribir a nadie (para ACTIVIDAD_SIN_ASISTENTES)

        await context.ActividadParticipantes.AddRangeAsync(inscripciones);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Inscripciones creadas exitosamente");
    }

    private static async Task SeedAsistenciasAsync(ApplicationDbContext context)
    {
        Console.WriteLine("✅ Seeding Asistencias...");

        if (await context.Asistencias.AnyAsync())
        {
            Console.WriteLine("⏭️  Asistencias ya existen, saltando...");
            return;
        }

        var actividades = await context.Actividades.Include(a => a.Programa).ToListAsync();
        var participantes = await context.Participantes.Include(p => p.Persona).ToListAsync();

        var asistencias = new List<Asistencia>();

        // EDV participantes
        var pEDV01 = participantes.First(p => p.Persona.Nombres == "Ana Sofía");
        var pEDV02 = participantes.First(p => p.Persona.Nombres == "Luis Alberto");
        var pEDV03 = participantes.First(p => p.Persona.Nombres == "Camila");

        var actEDV01 = actividades.First(a => a.Titulo == "EDV Taller 101");
        var actEDV02 = actividades.First(a => a.Titulo == "EDV Taller 102");
        var actEDV03 = actividades.First(a => a.Titulo == "EDV Taller 103");

        // P-EDV-01: 3 ausencias consecutivas
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV01.ActividadId,
            ParticipanteId = pEDV01.ParticipanteId,
            Fecha = new DateTime(2025, 10, 1),
            Estado = EstadoAsistencia.Ausente,
            CreadoEn = DateTime.UtcNow
        });
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV02.ActividadId,
            ParticipanteId = pEDV01.ParticipanteId,
            Fecha = new DateTime(2025, 10, 8),
            Estado = EstadoAsistencia.Ausente,
            CreadoEn = DateTime.UtcNow
        });
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV03.ActividadId,
            ParticipanteId = pEDV01.ParticipanteId,
            Fecha = new DateTime(2025, 10, 15),
            Estado = EstadoAsistencia.Ausente,
            CreadoEn = DateTime.UtcNow
        });

        // P-EDV-02: 1 presente, 2 ausentes (33.3% < 75%)
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV01.ActividadId,
            ParticipanteId = pEDV02.ParticipanteId,
            Fecha = new DateTime(2025, 10, 1),
            Estado = EstadoAsistencia.Presente,
            CreadoEn = DateTime.UtcNow
        });
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV02.ActividadId,
            ParticipanteId = pEDV02.ParticipanteId,
            Fecha = new DateTime(2025, 10, 8),
            Estado = EstadoAsistencia.Ausente,
            CreadoEn = DateTime.UtcNow
        });
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV03.ActividadId,
            ParticipanteId = pEDV02.ParticipanteId,
            Fecha = new DateTime(2025, 10, 15),
            Estado = EstadoAsistencia.Ausente,
            CreadoEn = DateTime.UtcNow
        });

        // P-EDV-03: Control (sin alerta) - 2 presentes, 1 tarde
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV01.ActividadId,
            ParticipanteId = pEDV03.ParticipanteId,
            Fecha = new DateTime(2025, 10, 1),
            Estado = EstadoAsistencia.Presente,
            CreadoEn = DateTime.UtcNow
        });
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV02.ActividadId,
            ParticipanteId = pEDV03.ParticipanteId,
            Fecha = new DateTime(2025, 10, 8),
            Estado = EstadoAsistencia.Presente,
            CreadoEn = DateTime.UtcNow
        });
        asistencias.Add(new Asistencia
        {
            ActividadId = actEDV03.ActividadId,
            ParticipanteId = pEDV03.ParticipanteId,
            Fecha = new DateTime(2025, 10, 15),
            Estado = EstadoAsistencia.Tarde,
            CreadoEn = DateTime.UtcNow
        });

        await context.Asistencias.AddRangeAsync(asistencias);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Asistencias creadas exitosamente");
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
        var progEDV = programas.First(p => p.Clave == "EDV");
        var progACADEMIA = programas.First(p => p.Clave == "ACADEMIA");

        var metricas = new List<MetricasProgramaMes>
        {
          // EDV - octubre 2025 (GAP = 50%)
     new MetricasProgramaMes
       {
     ProgramaId = progEDV.ProgramaId,
     AnioMes = "2025-10",
            ActividadesPlanificadas = 12,
        ActividadesEjecutadas = 6,
         PorcCumplimiento = 50.00m,
                RetrasoPromedioDias = 2.0m,
   PorcAsistenciaProm = 60.00m,
    CreadoEn = DateTime.UtcNow
            },
   // ACADEMIA - octubre 2025 (GAP = 30%)
  new MetricasProgramaMes
      {
      ProgramaId = progACADEMIA.ProgramaId,
     AnioMes = "2025-10",
         ActividadesPlanificadas = 10,
       ActividadesEjecutadas = 7,
          PorcCumplimiento = 70.00m,
    RetrasoPromedioDias = 1.0m,
                PorcAsistenciaProm = 65.00m,
   CreadoEn = DateTime.UtcNow
}
        };

        await context.MetricasProgramaMes.AddRangeAsync(metricas);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Métricas mensuales creadas exitosamente");
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

        // Campos para EDV
        var campoEDVPlan = new POACampo
        {
            PlantillaId = plantillaEDV.PlantillaId,
            Clave = "TALLERES_PLAN",
            Etiqueta = "Talleres Planificados",
            TipoDato = TipoDato.Entero,
            Requerido = true,
            Orden = 1,
            Alcance = AlcancePOA.Programa,
            CreadoEn = DateTime.UtcNow
        };

        var campoEDVEjec = new POACampo
        {
            PlantillaId = plantillaEDV.PlantillaId,
            Clave = "TALLERES_EJEC",
            Etiqueta = "Talleres Ejecutados",
            TipoDato = TipoDato.Entero,
            Requerido = true,
            Orden = 2,
            Alcance = AlcancePOA.Programa,
            CreadoEn = DateTime.UtcNow
        };

        // Campos para ACADEMIA
        var campoACADEMIAPlan = new POACampo
        {
            PlantillaId = plantillaACADEMIA.PlantillaId,
            Clave = "SESIONES_PLAN",
            Etiqueta = "Sesiones Planificadas",
            TipoDato = TipoDato.Entero,
            Requerido = true,
            Orden = 1,
            Alcance = AlcancePOA.Programa,
            CreadoEn = DateTime.UtcNow
        };

        var campoACADEMIAEjec = new POACampo
        {
            PlantillaId = plantillaACADEMIA.PlantillaId,
            Clave = "SESIONES_EJEC",
            Etiqueta = "Sesiones Ejecutadas",
            TipoDato = TipoDato.Entero,
            Requerido = true,
            Orden = 2,
            Alcance = AlcancePOA.Programa,
            CreadoEn = DateTime.UtcNow
        };

        await context.POACampos.AddRangeAsync(new[] { campoEDVPlan, campoEDVEjec, campoACADEMIAPlan, campoACADEMIAEjec });
        await context.SaveChangesAsync();

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

        // Valores
        var valores = new List<POAValor>
        {
            // EDV
     new POAValor
   {
     InstanciaId = instanciaEDV.InstanciaId,
    CampoId = campoEDVPlan.CampoId,
           ProgramaId = progEDV.ProgramaId,
     ValorNumero = 12,
       CreadoEn = DateTime.UtcNow
     },
  new POAValor
            {
                InstanciaId = instanciaEDV.InstanciaId,
 CampoId = campoEDVEjec.CampoId,
                ProgramaId = progEDV.ProgramaId,
      ValorNumero = 6,
       CreadoEn = DateTime.UtcNow
            },
            // ACADEMIA
 new POAValor
  {
                InstanciaId = instanciaACADEMIA.InstanciaId,
       CampoId = campoACADEMIAPlan.CampoId,
              ProgramaId = progACADEMIA.ProgramaId,
         ValorNumero = 10,
                CreadoEn = DateTime.UtcNow
            },
   new POAValor
    {
        InstanciaId = instanciaACADEMIA.InstanciaId,
    CampoId = campoACADEMIAEjec.CampoId,
    ProgramaId = progACADEMIA.ProgramaId,
   ValorNumero = 7,
        CreadoEn = DateTime.UtcNow
      }
        };

        await context.POAValores.AddRangeAsync(valores);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ POA Dinámico creado exitosamente");
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
                // EDV y ACADEMIA
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
                // Solo EDV
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

        Console.WriteLine("✅ Diccionario de Observaciones creado exitosamente");
    }
}
