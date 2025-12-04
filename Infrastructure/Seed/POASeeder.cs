using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Seed;

/// <summary>
/// Seeder dedicado para configurar las plantillas, campos e instancias POA
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

        // 1. Crear/actualizar plantillas y campos
        foreach (var programa in programas)
        {
            await CrearOActualizarPlantillaPOA(context, programa.ProgramaId, programa.Clave, programa.Nombre);
        }

        // 2. Crear instancias de POA con datos de ejemplo
        await CrearInstanciasPOAAsync(context);

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

        // ========== SECCIÓN: ACTIVIDADES =========
        if (!camposExistentes.Contains("ACTIVIDADES_PLANIFICADAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "ACTIVIDADES_PLANIFICADAS", "Actividades Planificadas", TipoDato.Entero, true, orden++));
        }
        if (!camposExistentes.Contains("ACTIVIDADES_EJECUTADAS"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "ACTIVIDADES_EJECUTADAS", "Actividades Ejecutadas", TipoDato.Entero, false, orden++));
        }

        // ========== SECCIÓN: PRESUPUESTO =========
        if (!camposExistentes.Contains("PRESUPUESTO_TOTAL"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PRESUPUESTO_TOTAL", "Presupuesto Total", TipoDato.Decimal, false, orden++, "USD"));
        }
        if (!camposExistentes.Contains("PRESUPUESTO_EJECUTADO"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PRESUPUESTO_EJECUTADO", "Presupuesto Ejecutado", TipoDato.Decimal, false, orden++, "USD"));
        }

        // ========== SECCIÓN: PARTICIPANTES =========
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

        // ========== SECCIÓN: RECURSOS HUMANOS =========
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

        // ========== SECCIÓN: IMPACTO Y RESULTADOS =========
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

        // ========== SECCIÓN: ALIANZAS Y COLABORACIONES =========
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

        // ========== SECCIÓN: FORMACIÓN Y CAPACITACIÓN =========
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

        // ========== SECCIÓN: INDICADORES =========
        if (!camposExistentes.Contains("PORCENTAJE_ASISTENCIA"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PORCENTAJE_ASISTENCIA", "Porcentaje de Asistencia", TipoDato.Decimal, false, orden++, "%"));
        }
        if (!camposExistentes.Contains("PORCENTAJE_CUMPLIMIENTO"))
        {
            camposNuevos.Add(CrearCampo(plantilla.PlantillaId, "PORCENTAJE_CUMPLIMIENTO", "Porcentaje de Cumplimiento", TipoDato.Decimal, false, orden++, "%"));
        }

        // ========== SECCIÓN: ANÁLISIS CUALITATIVO =========
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
        }
        else
        {
            Console.WriteLine($"    ✓ Todos los campos ya existen");
        }
    }

    /// <summary>
    /// Crear instancias de POA con valores calculados desde los datos existentes
    /// </summary>
    private static async Task CrearInstanciasPOAAsync(ApplicationDbContext context)
    {
        Console.WriteLine("\n📊 Creando instancias de POA con datos...");

        if (await context.POAInstancias.AnyAsync(i => !i.IsDeleted))
        {
            Console.WriteLine("⏭️  Instancias POA ya existen, saltando...");
            return;
        }

        var programas = await context.Programas.Where(p => !p.IsDeleted).ToListAsync();
        var random = new Random(42);

        // Crear POAs para los últimos 3 meses
        var fechaActual = DateTime.Now;
        var meses = new[] { 
            (fechaActual.Year, fechaActual.Month),
            (fechaActual.AddMonths(-1).Year, fechaActual.AddMonths(-1).Month),
            (fechaActual.AddMonths(-2).Year, fechaActual.AddMonths(-2).Month)
        };

        foreach (var programa in programas)
        {
            var plantilla = await context.POAPlantillas
                .Include(p => p.Campos)
                .FirstOrDefaultAsync(p => p.ProgramaId == programa.ProgramaId && 
                                         p.Estado == EstadoPlantilla.Activa && 
                                         !p.IsDeleted);

            if (plantilla == null)
            {
                Console.WriteLine($"  ⚠️ Sin plantilla activa para {programa.Nombre}");
                continue;
            }

            foreach (var (anio, mes) in meses)
            {
                await CrearInstanciaPOA(context, programa.ProgramaId, plantilla, (short)anio, (byte)mes, random);
            }
        }

        Console.WriteLine($"✅ Instancias POA creadas exitosamente");
    }

    private static async Task CrearInstanciaPOA(
        ApplicationDbContext context, 
        int programaId, 
        POAPlantilla plantilla, 
        short anio, 
        byte mes,
        Random random)
    {
        // Calcular datos reales del periodo
        var fechaInicio = new DateTime(anio, mes, 1);
        var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

        // Obtener actividades del periodo
        var actividades = await context.Actividades
            .Where(a => a.ProgramaId == programaId && 
                       a.FechaInicio >= fechaInicio && 
                       a.FechaInicio <= fechaFin &&
                       !a.IsDeleted)
            .ToListAsync();

        var actividadesPlanificadas = actividades.Count;
        var actividadesEjecutadas = actividades.Count(a => a.Estado == EstadoActividad.Realizada);

        // Obtener participantes únicos del periodo
        var participantesIds = await context.ActividadParticipantes
            .Include(ap => ap.Actividad)
            .Include(ap => ap.Participante)
            .Where(ap => ap.Actividad.ProgramaId == programaId && 
                        ap.Actividad.FechaInicio >= fechaInicio && 
                        ap.Actividad.FechaInicio <= fechaFin &&
                        !ap.IsDeleted)
            .Select(ap => new { ap.ParticipanteId, ap.Participante.Estado })
            .Distinct()
            .ToListAsync();

        var totalParticipantes = participantesIds.Count;
        var participantesActivos = participantesIds.Count(p => p.Estado == EstadoGeneral.Activo);

        // Calcular asistencia promedio
        var asistencias = await context.Asistencias
            .Include(a => a.Actividad)
            .Where(a => a.Actividad.ProgramaId == programaId && 
                       a.Fecha >= fechaInicio && 
                       a.Fecha <= fechaFin &&
                       !a.IsDeleted)
            .ToListAsync();

        var totalAsistencias = asistencias.Count;
        var presentes = asistencias.Count(a => a.Estado == EstadoAsistencia.Presente || a.Estado == EstadoAsistencia.Tarde);
        var porcentajeAsistencia = totalAsistencias > 0 ? (presentes * 100.0m / totalAsistencias) : 0;
        var porcentajeCumplimiento = actividadesPlanificadas > 0 ? (actividadesEjecutadas * 100.0m / actividadesPlanificadas) : 0;

        // Crear la instancia
        var instancia = new POAInstancia
        {
            ProgramaId = programaId,
            PlantillaId = plantilla.PlantillaId,
            PeriodoAnio = anio,
            PeriodoMes = mes,
            Estado = mes == DateTime.Now.Month ? EstadoInstancia.Borrador : EstadoInstancia.Aprobado,
            Notas = $"POA generado automáticamente para {ObtenerNombreMes(mes)} {anio}",
            CreadoEn = DateTime.UtcNow
        };

        context.POAInstancias.Add(instancia);
        await context.SaveChangesAsync();

        // Crear valores para cada campo
        var valores = new List<POAValor>();
        var campos = plantilla.Campos.ToDictionary(c => c.Clave, c => c.CampoId);

        // Generar datos simulados para campos que no tienen datos reales
        var presupuestoTotal = random.Next(5000, 20000) * 100m; // Entre 500,000 y 2,000,000
        var presupuestoEjecutado = presupuestoTotal * (decimal)(0.5 + random.NextDouble() * 0.4); // 50-90% ejecutado

        // Mapear valores
        var valoresData = new Dictionary<string, (int? Numero, decimal? Decimal, string? Texto)>
        {
            // Actividades
            ["ACTIVIDADES_PLANIFICADAS"] = (actividadesPlanificadas > 0 ? actividadesPlanificadas : random.Next(8, 20), null, null),
            ["ACTIVIDADES_EJECUTADAS"] = (actividadesEjecutadas > 0 ? actividadesEjecutadas : random.Next(5, 15), null, null),
            
            // Presupuesto
            ["PRESUPUESTO_TOTAL"] = (null, presupuestoTotal, null),
            ["PRESUPUESTO_EJECUTADO"] = (null, presupuestoEjecutado, null),
            
            // Participantes
            ["TOTAL_PARTICIPANTES"] = (totalParticipantes > 0 ? totalParticipantes : random.Next(30, 80), null, null),
            ["PARTICIPANTES_ACTIVOS"] = (participantesActivos > 0 ? participantesActivos : random.Next(25, 70), null, null),
            ["NUEVOS_PARTICIPANTES"] = (random.Next(2, 10), null, null),
            ["PARTICIPANTES_RETIRADOS"] = (random.Next(0, 5), null, null),
            
            // Recursos Humanos
            ["FACILITADORES"] = (random.Next(3, 8), null, null),
            ["VOLUNTARIOS"] = (random.Next(5, 15), null, null),
            ["HORAS_VOLUNTARIADO"] = (random.Next(50, 200), null, null),
            
            // Impacto
            ["OBJETIVOS_TOTALES"] = (random.Next(5, 12), null, null),
            ["OBJETIVOS_CUMPLIDOS"] = (random.Next(3, 10), null, null),
            ["FAMILIAS_IMPACTADAS"] = (random.Next(20, 60), null, null),
            ["COMUNIDADES_ALCANZADAS"] = (random.Next(2, 8), null, null),
            
            // Alianzas
            ["ALIANZAS_ACTIVAS"] = (random.Next(3, 10), null, null),
            ["NUEVAS_ALIANZAS"] = (random.Next(0, 3), null, null),
            ["APORTES_ESPECIE"] = (null, random.Next(1000, 10000), null),
            
            // Capacitaciones
            ["CAPACITACIONES_REALIZADAS"] = (random.Next(2, 8), null, null),
            ["PERSONAS_CAPACITADAS"] = (random.Next(15, 50), null, null),
            ["CERTIFICADOS_EMITIDOS"] = (random.Next(10, 40), null, null),
            
            // Indicadores
            ["PORCENTAJE_ASISTENCIA"] = (null, Math.Round(porcentajeAsistencia > 0 ? porcentajeAsistencia : random.Next(70, 95), 2), null),
            ["PORCENTAJE_CUMPLIMIENTO"] = (null, Math.Round(porcentajeCumplimiento > 0 ? porcentajeCumplimiento : random.Next(60, 95), 2), null),
            
            // Narrativa
            ["LOGROS_PRINCIPALES"] = (null, null, GenerarLogros(random)),
            ["RETOS_ENFRENTADOS"] = (null, null, GenerarRetos(random)),
            ["LECCIONES_APRENDIDAS"] = (null, null, GenerarLecciones(random)),
            ["PROXIMOS_PASOS"] = (null, null, GenerarProximosPasos(random))
        };

        foreach (var (clave, (numero, dec, texto)) in valoresData)
        {
            if (campos.TryGetValue(clave, out var campoId))
            {
                valores.Add(new POAValor
                {
                    InstanciaId = instancia.InstanciaId,
                    CampoId = campoId,
                    ProgramaId = programaId,
                    ValorNumero = numero,
                    ValorDecimal = dec ?? numero,
                    ValorTexto = texto,
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        if (valores.Any())
        {
            context.POAValores.AddRange(valores);
            await context.SaveChangesAsync();
        }

        Console.WriteLine($"  ✓ POA {ObtenerNombreMes(mes)} {anio} para programa {programaId}: {valores.Count} valores");
    }

    private static string GenerarLogros(Random random)
    {
        var logros = new[]
        {
            "Se logró incrementar la participación de jóvenes en un 15% respecto al mes anterior.",
            "Se completaron todas las actividades planificadas con alta asistencia.",
            "Se establecieron 2 nuevas alianzas estratégicas con organizaciones locales.",
            "Los participantes mostraron mejoras significativas en habilidades de liderazgo.",
            "Se capacitó exitosamente a nuevos facilitadores del programa."
        };
        return logros[random.Next(logros.Length)];
    }

    private static string GenerarRetos(Random random)
    {
        var retos = new[]
        {
            "Dificultades con el transporte de algunos participantes a las actividades.",
            "Limitaciones presupuestarias para materiales didácticos.",
            "Coordinación de horarios con instituciones educativas aliadas.",
            "Necesidad de más facilitadores capacitados para cubrir la demanda.",
            "Condiciones climáticas afectaron algunas actividades al aire libre."
        };
        return retos[random.Next(retos.Length)];
    }

    private static string GenerarLecciones(Random random)
    {
        var lecciones = new[]
        {
            "La comunicación constante con las familias mejora la asistencia.",
            "Las actividades prácticas generan mayor engagement que las teóricas.",
            "Es importante tener planes de contingencia para eventos externos.",
            "El trabajo en equipo entre facilitadores optimiza los resultados.",
            "La flexibilidad en los horarios aumenta la participación."
        };
        return lecciones[random.Next(lecciones.Length)];
    }

    private static string GenerarProximosPasos(Random random)
    {
        var pasos = new[]
        {
            "Implementar nuevas metodologías de enseñanza participativa.",
            "Expandir el programa a dos comunidades adicionales.",
            "Realizar evaluación de impacto con los participantes graduados.",
            "Fortalecer las alianzas existentes con reuniones mensuales.",
            "Capacitar a 5 nuevos voluntarios para el próximo periodo."
        };
        return pasos[random.Next(pasos.Length)];
    }

    private static string ObtenerNombreMes(int mes)
    {
        return mes switch
        {
            1 => "Enero", 2 => "Febrero", 3 => "Marzo", 4 => "Abril",
            5 => "Mayo", 6 => "Junio", 7 => "Julio", 8 => "Agosto",
            9 => "Septiembre", 10 => "Octubre", 11 => "Noviembre", 12 => "Diciembre",
            _ => "N/A"
        };
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

        // Mostrar instancias creadas
        var instancias = await context.POAInstancias
            .Include(i => i.Programa)
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.PeriodoAnio)
            .ThenByDescending(i => i.PeriodoMes)
            .ToListAsync();

        if (instancias.Any())
        {
            Console.WriteLine("📋 Instancias POA creadas:");
            foreach (var inst in instancias)
            {
                var mesNombre = inst.PeriodoMes.HasValue ? ObtenerNombreMes(inst.PeriodoMes.Value) : "Anual";
                Console.WriteLine($"   - {inst.Programa?.Nombre}: {mesNombre} {inst.PeriodoAnio} [{inst.Estado}]");
            }
        }
    }
}
