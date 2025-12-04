using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder dedicado para configurar las plantillas y campos POA
/// </summary>
public static class POASeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        Console.WriteLine("📋 Iniciando POA Seeder...");

        // Obtener programas existentes
        var programas = await context.Programas.Where(p => !p.IsDeleted).ToListAsync();
        
        if (!programas.Any())
        {
            Console.WriteLine("⚠️ No hay programas disponibles. Ejecute primero el DatabaseSeeder.");
            return;
        }

        foreach (var programa in programas)
        {
            await CrearOActualizarPlantillaPOA(context, programa.ProgramaId, programa.Clave, programa.Nombre);
        }

        Console.WriteLine("✅ POA Seeder completado exitosamente!");
    }

    private static async Task CrearOActualizarPlantillaPOA(ApplicationDbContext context, int programaId, string clave, string nombre)
    {
        Console.WriteLine($"  📄 Procesando plantilla para {nombre} ({clave})...");

        // Verificar si ya existe una plantilla activa
        var plantillaExistente = await context.POAPlantillas
            .Include(p => p.Campos)
            .FirstOrDefaultAsync(p => p.ProgramaId == programaId && p.Estado == EstadoPlantilla.Activa && !p.IsDeleted);

        POAPlantilla plantilla;

        if (plantillaExistente != null)
        {
            Console.WriteLine($"    ✓ Plantilla existente encontrada (ID: {plantillaExistente.PlantillaId})");
            plantilla = plantillaExistente;
        }
        else
        {
            // Crear nueva plantilla
            plantilla = new POAPlantilla
            {
                ProgramaId = programaId,
                Version = 1,
                Estado = EstadoPlantilla.Activa,
                VigenteDesde = DateTime.UtcNow,
                CreadoEn = DateTime.UtcNow
            };

            context.POAPlantillas.Add(plantilla);
            await context.SaveChangesAsync();
            Console.WriteLine($"    + Nueva plantilla creada (ID: {plantilla.PlantillaId})");
        }

        // Agregar campos faltantes
        await AgregarCamposFaltantes(context, plantilla);
    }

    private static async Task AgregarCamposFaltantes(ApplicationDbContext context, POAPlantilla plantilla)
    {
        var camposExistentes = await context.POACampos
            .Where(c => c.PlantillaId == plantilla.PlantillaId && !c.IsDeleted)
            .Select(c => c.Clave)
            .ToListAsync();

        var camposNuevos = new List<POACampo>();
        var orden = camposExistentes.Count + 1;

        // ========== SECCIÓN: ACTIVIDADES ==========
        if (!camposExistentes.Contains("ACTIVIDADES_PLANIFICADAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "ACTIVIDADES_PLANIFICADAS", "Actividades Planificadas", TipoDato.Entero, true, orden++));
        }
        if (!camposExistentes.Contains("ACTIVIDADES_EJECUTADAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "ACTIVIDADES_EJECUTADAS", "Actividades Ejecutadas", TipoDato.Entero, false, orden++));
        }

        // ========== SECCIÓN: PRESUPUESTO ==========
        if (!camposExistentes.Contains("PRESUPUESTO_TOTAL"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PRESUPUESTO_TOTAL", "Presupuesto Total", TipoDato.Decimal, false, orden++, "USD"));
        }
        if (!camposExistentes.Contains("PRESUPUESTO_EJECUTADO"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PRESUPUESTO_EJECUTADO", "Presupuesto Ejecutado", TipoDato.Decimal, false, orden++, "USD"));
        }

        // ========== SECCIÓN: PARTICIPANTES ==========
        if (!camposExistentes.Contains("TOTAL_PARTICIPANTES"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "TOTAL_PARTICIPANTES", "Total Participantes", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("PARTICIPANTES_ACTIVOS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PARTICIPANTES_ACTIVOS", "Participantes Activos", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("NUEVOS_PARTICIPANTES"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "NUEVOS_PARTICIPANTES", "Nuevos Participantes", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("PARTICIPANTES_RETIRADOS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PARTICIPANTES_RETIRADOS", "Participantes Retirados", TipoDato.Entero, false, orden++));
        }

        // ========== SECCIÓN: RECURSOS HUMANOS ==========
        if (!camposExistentes.Contains("FACILITADORES"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "FACILITADORES", "Total Facilitadores", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("VOLUNTARIOS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "VOLUNTARIOS", "Total Voluntarios", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("HORAS_VOLUNTARIADO"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "HORAS_VOLUNTARIADO", "Horas de Voluntariado", TipoDato.Entero, false, orden++));
        }

        // ========== SECCIÓN: IMPACTO Y RESULTADOS ==========
        if (!camposExistentes.Contains("OBJETIVOS_CUMPLIDOS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "OBJETIVOS_CUMPLIDOS", "Objetivos Cumplidos", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("OBJETIVOS_TOTALES"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "OBJETIVOS_TOTALES", "Objetivos Totales", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("FAMILIAS_IMPACTADAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "FAMILIAS_IMPACTADAS", "Familias Impactadas", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("COMUNIDADES_ALCANZADAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "COMUNIDADES_ALCANZADAS", "Comunidades Alcanzadas", TipoDato.Entero, false, orden++));
        }

        // ========== SECCIÓN: ALIANZAS Y COLABORACIONES ==========
        if (!camposExistentes.Contains("ALIANZAS_ACTIVAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "ALIANZAS_ACTIVAS", "Alianzas Activas", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("NUEVAS_ALIANZAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "NUEVAS_ALIANZAS", "Nuevas Alianzas", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("APORTES_ESPECIE"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "APORTES_ESPECIE", "Aportes en Especie", TipoDato.Decimal, false, orden++, "USD"));
        }

        // ========== SECCIÓN: FORMACIÓN Y CAPACITACIÓN ==========
        if (!camposExistentes.Contains("CAPACITACIONES_REALIZADAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "CAPACITACIONES_REALIZADAS", "Capacitaciones Realizadas", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("PERSONAS_CAPACITADAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PERSONAS_CAPACITADAS", "Personas Capacitadas", TipoDato.Entero, false, orden++));
        }
        if (!camposExistentes.Contains("CERTIFICADOS_EMITIDOS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "CERTIFICADOS_EMITIDOS", "Certificados Emitidos", TipoDato.Entero, false, orden++));
        }

        // ========== SECCIÓN: INDICADORES ==========
        if (!camposExistentes.Contains("PORCENTAJE_ASISTENCIA"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PORCENTAJE_ASISTENCIA", "Porcentaje de Asistencia", TipoDato.Decimal, false, orden++, "%"));
        }
        if (!camposExistentes.Contains("PORCENTAJE_CUMPLIMIENTO"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PORCENTAJE_CUMPLIMIENTO", "Porcentaje de Cumplimiento", TipoDato.Decimal, false, orden++, "%"));
        }

        // ========== SECCIÓN: ANÁLISIS CUALITATIVO ==========
        if (!camposExistentes.Contains("LOGROS_PRINCIPALES"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "LOGROS_PRINCIPALES", "Logros Principales", TipoDato.Texto, false, orden++));
        }
        if (!camposExistentes.Contains("RETOS_ENFRENTADOS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "RETOS_ENFRENTADOS", "Retos Enfrentados", TipoDato.Texto, false, orden++));
        }
        if (!camposExistentes.Contains("LECCIONES_APRENDIDAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "LECCIONES_APRENDIDAS", "Lecciones Aprendidas", TipoDato.Texto, false, orden++));
        }
        if (!camposExistentes.Contains("PROXIMOS_PASOS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PROXIMOS_PASOS", "Próximos Pasos", TipoDato.Texto, false, orden++));
        }

        // Guardar nuevos campos
        if (camposNuevos.Any())
        {
            context.POACampos.AddRange(camposNuevos);
            await context.SaveChangesAsync();
            Console.WriteLine($"    + {camposNuevos.Count} campos agregados");
            
            foreach (var campo in camposNuevos)
            {
                Console.WriteLine($"      - {campo.Clave}: {campo.Etiqueta}");
            }
        }
        else
        {
            Console.WriteLine($"    ✓ Todos los campos ya existen");
        }
    }

    private static POACampo CrearCampo(int plantillaId, string clave, string etiqueta, TipoDato tipoDato, bool requerido, int orden, string? unidad = null)
    {
        return new POACampo
        {
            PlantillaId = plantillaId,
            Clave = clave,
            Etiqueta = etiqueta,
            TipoDato = tipoDato,
            Requerido = requerido,
            Orden = orden,
            Unidad = unidad,
            Alcance = AlcancePOA.Programa,
            CreadoEn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Verifica el estado de los campos POA en la base de datos
    /// </summary>
    public static async Task VerificarEstadoPOAAsync(ApplicationDbContext context)
    {
        Console.WriteLine("\n📊 === ESTADO DE CAMPOS POA ===\n");

        var plantillas = await context.POAPlantillas
            .Include(p => p.Programa)
            .Include(p => p.Campos)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        if (!plantillas.Any())
        {
            Console.WriteLine("⚠️ No hay plantillas POA configuradas.");
            return;
        }

        foreach (var plantilla in plantillas)
        {
            Console.WriteLine($"📄 Plantilla: {plantilla.Programa?.Nombre ?? "Sin programa"} (ID: {plantilla.PlantillaId})");
            Console.WriteLine($"   Estado: {plantilla.Estado} | Versión: {plantilla.Version}");
            Console.WriteLine($"   Campos configurados: {plantilla.Campos.Count(c => !c.IsDeleted)}");
            Console.WriteLine();

            var campos = plantilla.Campos.Where(c => !c.IsDeleted).OrderBy(c => c.Orden).ToList();
            
            foreach (var campo in campos)
            {
            var tipoStr = campo.TipoDato switch
            {
                TipoDato.Texto => "📝",
                TipoDato.Entero => "🔢",
                TipoDato.Decimal => "💰",
                TipoDato.Fecha => "📅",
                TipoDato.Bool => "✓/✗",
                TipoDato.Lista => "📋",
                _ => "❓"
            };
                
                Console.WriteLine($"   {campo.Orden:00}. {tipoStr} {campo.Clave} - {campo.Etiqueta}" + 
                    (campo.Requerido ? " *" : "") +
                    (!string.IsNullOrEmpty(campo.Unidad) ? $" ({campo.Unidad})" : ""));
            }
            
            Console.WriteLine();
        }

        // Lista de campos esperados
        var camposEsperados = new[]
        {
            "ACTIVIDADES_PLANIFICADAS", "ACTIVIDADES_EJECUTADAS",
            "PRESUPUESTO_TOTAL", "PRESUPUESTO_EJECUTADO",
            "TOTAL_PARTICIPANTES", "PARTICIPANTES_ACTIVOS", "NUEVOS_PARTICIPANTES", "PARTICIPANTES_RETIRADOS",
            "FACILITADORES", "VOLUNTARIOS", "HORAS_VOLUNTARIADO",
            "OBJETIVOS_CUMPLIDOS", "OBJETIVOS_TOTALES", "FAMILIAS_IMPACTADAS", "COMUNIDADES_ALCANZADAS",
            "ALIANZAS_ACTIVAS", "NUEVAS_ALIANZAS", "APORTES_ESPECIE",
            "CAPACITACIONES_REALIZADAS", "PERSONAS_CAPACITADAS", "CERTIFICADOS_EMITIDOS",
            "PORCENTAJE_ASISTENCIA", "PORCENTAJE_CUMPLIMIENTO",
            "LOGROS_PRINCIPALES", "RETOS_ENFRENTADOS", "LECCIONES_APRENDIDAS", "PROXIMOS_PASOS"
        };

        Console.WriteLine("📋 Verificación de campos esperados:");
        
        foreach (var plantilla in plantillas)
        {
            var camposPlantilla = plantilla.Campos.Where(c => !c.IsDeleted).Select(c => c.Clave).ToHashSet();
            var faltantes = camposEsperados.Where(c => !camposPlantilla.Contains(c)).ToList();
            
            if (faltantes.Any())
            {
                Console.WriteLine($"\n   ⚠️ {plantilla.Programa?.Nombre}: Faltan {faltantes.Count} campos:");
                foreach (var faltante in faltantes)
                {
                    Console.WriteLine($"      - {faltante}");
                }
            }
            else
            {
                Console.WriteLine($"\n   ✅ {plantilla.Programa?.Nombre}: Todos los campos configurados");
            }
        }
    }
}
