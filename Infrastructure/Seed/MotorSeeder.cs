using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Config;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder para Configuración del Motor de Inferencia, Reglas y Parámetros
/// </summary>
public static class MotorSeeder
{
    public static async Task SeedConfiguracionAsync(ApplicationDbContext context)
    {
        Console.WriteLine("??  Seeding Configuración del Motor...");

        if (await context.ConfiguracionesMotor.AnyAsync())
        {
            Console.WriteLine("??  Configuraciones ya existen, saltando...");
            return;
        }

        var configuraciones = new List<ConfiguracionMotor>
        {
            new() { Clave = "ASISTENCIA_MIN_PORCENTAJE", Valor = "75", Descripcion = "Porcentaje mínimo de asistencia requerido", Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "DIAS_ALERTA_INASISTENCIA", Valor = "7", Descripcion = "Días consecutivos de inasistencia para generar alerta", Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "UMBRAL_RIESGO_BAJO", Valor = "30", Descripcion = "Score máximo para riesgo bajo", Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "UMBRAL_RIESGO_MEDIO", Valor = "60", Descripcion = "Score máximo para riesgo medio", Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "MOTOR_EJECUCION_AUTO", Valor = "true", Descripcion = "Ejecutar motor de inferencia automáticamente", Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "FRECUENCIA_EJECUCION_HORAS", Valor = "24", Descripcion = "Frecuencia de ejecución del motor en horas", Version = 1, CreadoEn = DateTime.UtcNow }
        };

        await context.ConfiguracionesMotor.AddRangeAsync(configuraciones);
        await context.SaveChangesAsync();

        // Overrides por programa
        var programas = await context.Programas.ToListAsync();
        var progAcademia = programas.FirstOrDefault(p => p.Clave == "ACADEMIA");
        var progEDV = programas.FirstOrDefault(p => p.Clave == "EDV");

        var overrides = new List<ConfiguracionMotorOverride>();

        if (progAcademia != null)
        {
            overrides.Add(new ConfiguracionMotorOverride
            {
                ProgramaId = progAcademia.ProgramaId,
                Clave = "MOTOR_EJECUCION_AUTO",
                Valor = "true",
                Descripcion = "Override para ACADEMIA",
                Version = 1,
                CreadoEn = DateTime.UtcNow
            });
        }

        if (progEDV != null)
        {
            overrides.Add(new ConfiguracionMotorOverride
            {
                ProgramaId = progEDV.ProgramaId,
                Clave = "FRECUENCIA_EJECUCION_HORAS",
                Valor = "12",
                Descripcion = "Override para EDV",
                Version = 1,
                CreadoEn = DateTime.UtcNow
            });
        }

        if (overrides.Any())
        {
            await context.ConfiguracionMotorOverrides.AddRangeAsync(overrides);
            await context.SaveChangesAsync();
        }

        Console.WriteLine($"? {configuraciones.Count} Configuraciones del Motor creadas");
        Console.WriteLine($"   - {overrides.Count} Overrides por programa");
    }

    public static async Task SeedReglasAsync(ApplicationDbContext context)
    {
        Console.WriteLine("?? Seeding Reglas y Parámetros...");

        if (await context.Reglas.AnyAsync())
        {
            Console.WriteLine("??  Reglas ya existen, saltando...");
            return;
        }

        var reglas = new List<Regla>
        {
            new() { Clave = "INASISTENCIA_CONSECUTIVA", Nombre = "Inasistencia Consecutiva", Descripcion = "Detecta participantes con múltiples inasistencias seguidas", Severidad = Severidad.Alta, Objetivo = ObjetivoRegla.Participante, Activa = true, Prioridad = 100, Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "BAJA_ASISTENCIA_GENERAL", Nombre = "Baja Asistencia General", Descripcion = "Porcentaje de asistencia por debajo del umbral", Severidad = Severidad.Alta, Objetivo = ObjetivoRegla.Participante, Activa = true, Prioridad = 90, Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "ACTIVIDAD_SIN_ASISTENTES", Nombre = "Actividad sin asistentes", Descripcion = "Actividades planificadas sin inscritos", Severidad = Severidad.Info, Objetivo = ObjetivoRegla.Actividad, Activa = true, Prioridad = 50, Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "RETRASO_ACTIVIDAD", Nombre = "Retraso en actividad", Descripcion = "Actividad planificada no ejecutada pasada su fecha", Severidad = Severidad.Alta, Objetivo = ObjetivoRegla.Actividad, Activa = true, Prioridad = 80, Version = 1, CreadoEn = DateTime.UtcNow },
            new() { Clave = "BAJO_CUMPLIMIENTO_POA", Nombre = "Bajo Cumplimiento de POA", Descripcion = "Desvío plan vs ejecución mensual por encima del umbral", Severidad = Severidad.Critica, Objetivo = ObjetivoRegla.Programa, Activa = true, Prioridad = 100, Version = 1, CreadoEn = DateTime.UtcNow }
        };

        await context.Reglas.AddRangeAsync(reglas);
        await context.SaveChangesAsync();

        // Parámetros
        var reglaInasistencia = reglas.First(r => r.Clave == "INASISTENCIA_CONSECUTIVA");
        var reglaBajaAsistencia = reglas.First(r => r.Clave == "BAJA_ASISTENCIA_GENERAL");
        var reglaSinAsistentes = reglas.First(r => r.Clave == "ACTIVIDAD_SIN_ASISTENTES");
        var reglaRetraso = reglas.First(r => r.Clave == "RETRASO_ACTIVIDAD");
        var reglaBajoCumplimiento = reglas.First(r => r.Clave == "BAJO_CUMPLIMIENTO_POA");

        var parametros = new List<ReglaParametro>
        {
            new() { ReglaId = reglaInasistencia.ReglaId, Nombre = "UMBRAL_AUSENCIAS", Tipo = TipoParametro.Entero, Valor = "3", CreadoEn = DateTime.UtcNow },
            new() { ReglaId = reglaBajaAsistencia.ReglaId, Nombre = "UMBRAL_PCT", Tipo = TipoParametro.Decimal, Valor = "75", CreadoEn = DateTime.UtcNow },
            new() { ReglaId = reglaSinAsistentes.ReglaId, Nombre = "MIN_INSCRITOS", Tipo = TipoParametro.Entero, Valor = "1", CreadoEn = DateTime.UtcNow },
            new() { ReglaId = reglaRetraso.ReglaId, Nombre = "DIAS_RETRASO", Tipo = TipoParametro.Entero, Valor = "3", CreadoEn = DateTime.UtcNow },
            new() { ReglaId = reglaBajoCumplimiento.ReglaId, Nombre = "UMBRAL_PCT", Tipo = TipoParametro.Decimal, Valor = "20", CreadoEn = DateTime.UtcNow }
        };

        await context.ReglaParametros.AddRangeAsync(parametros);
        await context.SaveChangesAsync();

        // Overrides de parámetros
        var programas = await context.Programas.ToListAsync();
        var progAcademia = programas.FirstOrDefault(p => p.Clave == "ACADEMIA");
        var progEDV = programas.FirstOrDefault(p => p.Clave == "EDV");

        var overrides = new List<ReglaParametroOverride>();

        if (progAcademia != null)
        {
            overrides.Add(new ReglaParametroOverride
            {
                ReglaId = reglaBajoCumplimiento.ReglaId,
                ProgramaId = progAcademia.ProgramaId,
                Nombre = "UMBRAL_PCT",
                Tipo = TipoParametro.Decimal,
                Valor = "15",
                CreadoEn = DateTime.UtcNow
            });
        }

        if (progEDV != null)
        {
            overrides.Add(new ReglaParametroOverride
            {
                ReglaId = reglaInasistencia.ReglaId,
                ProgramaId = progEDV.ProgramaId,
                Nombre = "UMBRAL_AUSENCIAS",
                Tipo = TipoParametro.Entero,
                Valor = "2",
                CreadoEn = DateTime.UtcNow
            });
        }

        if (overrides.Any())
        {
            await context.ReglaParametroOverrides.AddRangeAsync(overrides);
            await context.SaveChangesAsync();
        }

        Console.WriteLine($"? {reglas.Count} Reglas creadas");
        Console.WriteLine($"   - {parametros.Count} Parámetros");
        Console.WriteLine($"   - {overrides.Count} Overrides");
    }

    public static async Task SeedDiccionarioObservacionesAsync(ApplicationDbContext context)
    {
        Console.WriteLine("?? Seeding Diccionario de Observaciones...");

        if (await context.DiccionarioObservaciones.AnyAsync())
        {
            Console.WriteLine("??  Diccionario ya existe, saltando...");
            return;
        }

        var diccionarios = new List<DiccionarioObservaciones>
        {
            new() { Expresion = "llegó tarde", Ponderacion = 0.2m, Ambito = AmbitoDiccionario.Global, Activo = true, CreadoEn = DateTime.UtcNow },
            new() { Expresion = "no asistió", Ponderacion = 0.5m, Ambito = AmbitoDiccionario.Global, Activo = true, CreadoEn = DateTime.UtcNow },
            new() { Expresion = "se retiró temprano", Ponderacion = 0.3m, Ambito = AmbitoDiccionario.Global, Activo = true, CreadoEn = DateTime.UtcNow },
            new() { Expresion = "poca participación", Ponderacion = 0.25m, Ambito = AmbitoDiccionario.Global, Activo = true, CreadoEn = DateTime.UtcNow }
        };

        await context.DiccionarioObservaciones.AddRangeAsync(diccionarios);
        await context.SaveChangesAsync();

        // Relaciones con programas
        var programas = await context.Programas.ToListAsync();
        var relaciones = new List<DiccionarioObservacionesPrograma>();

        foreach (var dic in diccionarios)
        {
            foreach (var prog in programas.Where(p => p.Clave == "EDV" || p.Clave == "ACADEMIA"))
            {
                relaciones.Add(new DiccionarioObservacionesPrograma
                {
                    DiccionarioId = dic.DiccionarioId,
                    ProgramaId = prog.ProgramaId,
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        await context.DiccionarioObservacionesProgramas.AddRangeAsync(relaciones);
        await context.SaveChangesAsync();

        Console.WriteLine($"? {diccionarios.Count} Expresiones en diccionario");
        Console.WriteLine($"   - {relaciones.Count} Relaciones con programas");
    }
}
