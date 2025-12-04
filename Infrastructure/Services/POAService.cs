using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Pages.POA;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services;

public class POAService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<POAService> _logger;

    public POAService(ApplicationDbContext context, ILogger<POAService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<POAViewModel>> ObtenerPOAsPorProgramaAsync(int programaId)
    {
        return await _context.POAInstancias
            .Where(p => p.ProgramaId == programaId && !p.IsDeleted)
            .Include(p => p.Programa)
            .OrderByDescending(p => p.PeriodoAnio)
            .ThenByDescending(p => p.PeriodoMes)
            .Select(p => new POAViewModel
            {
                InstanciaId = p.InstanciaId,
                ProgramaId = p.ProgramaId,
                ProgramaNombre = p.Programa.Nombre,
                ProgramaClave = p.Programa.Clave,
                PlantillaId = p.PlantillaId,
                PeriodoAnio = p.PeriodoAnio,
                PeriodoMes = p.PeriodoMes,
                Estado = p.Estado.ToString(),
                Notas = p.Notas,
                CreadoEn = p.CreadoEn,
                ActualizadoEn = p.ActualizadoEn
            })
            .ToListAsync();
    }

    public async Task<PlantillaPOAViewModel?> ObtenerPlantillaActivaPorProgramaAsync(int programaId)
    {
        var plantilla = await _context.POAPlantillas
            .Where(p => p.ProgramaId == programaId
                     && p.Estado == EstadoPlantilla.Activa
                     && !p.IsDeleted)
            .Include(p => p.Secciones)
            .Include(p => p.Campos)
                .ThenInclude(c => c.Opciones)
            .Include(p => p.Campos)
                .ThenInclude(c => c.Validaciones)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync();

        if (plantilla == null)
            return null;

        return new PlantillaPOAViewModel
        {
            PlantillaId = plantilla.PlantillaId,
            ProgramaId = plantilla.ProgramaId,
            Version = plantilla.Version,
            Estado = plantilla.Estado.ToString(),
            VigenteDesde = plantilla.VigenteDesde,
            VigenteHasta = plantilla.VigenteHasta,
            Secciones = plantilla.Secciones
                .OrderBy(s => s.Orden)
                .Select(s => new SeccionPOAViewModel
                {
                    SeccionId = s.SeccionId,
                    Nombre = s.Nombre,
                    Orden = s.Orden,
                    Campos = plantilla.Campos
                        .Where(c => c.SeccionId == s.SeccionId)
                        .OrderBy(c => c.Orden)
                        .Select(c => new CampoPOAViewModel
                        {
                            CampoId = c.CampoId,
                            Clave = c.Clave,
                            Etiqueta = c.Etiqueta,
                            TipoDato = c.TipoDato.ToString(),
                            Requerido = c.Requerido,
                            Orden = c.Orden,
                            Unidad = c.Unidad,
                            Alcance = c.Alcance.ToString(),
                            Opciones = c.Opciones.Select(o => new OpcionCampoViewModel
                            {
                                OpcionId = o.OpcionId,
                                Valor = o.Valor,
                                Etiqueta = o.Etiqueta
                            }).ToList(),
                            Validaciones = c.Validaciones.Select(v => new ValidacionCampoViewModel
                            {
                                ValidacionId = v.ValidacionId,
                                Tipo = v.Tipo.ToString(),
                                Parametro = v.Parametro
                            }).ToList()
                        }).ToList()
                }).ToList()
        };
    }

    public async Task<POAViewModel?> ObtenerPOAPorIdAsync(int instanciaId)
    {
        return await _context.POAInstancias
            .Where(p => p.InstanciaId == instanciaId && !p.IsDeleted)
            .Include(p => p.Programa)
            .Select(p => new POAViewModel
            {
                InstanciaId = p.InstanciaId,
                ProgramaId = p.ProgramaId,
                ProgramaNombre = p.Programa.Nombre,
                ProgramaClave = p.Programa.Clave,
                PlantillaId = p.PlantillaId,
                PeriodoAnio = p.PeriodoAnio,
                PeriodoMes = p.PeriodoMes,
                Estado = p.Estado.ToString(),
                Notas = p.Notas,
                CreadoEn = p.CreadoEn,
                ActualizadoEn = p.ActualizadoEn
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PlantillaPOAViewModel?> ObtenerPlantillaConValoresAsync(int instanciaId)
    {
        var instancia = await _context.POAInstancias
            .Include(p => p.Plantilla)
                .ThenInclude(pl => pl.Secciones)
            .Include(p => p.Plantilla)
                .ThenInclude(pl => pl.Campos)
                    .ThenInclude(c => c.Opciones)
            .Include(p => p.Plantilla)
                .ThenInclude(pl => pl.Campos)
                    .ThenInclude(c => c.Validaciones)
            .Include(p => p.Valores)
            .FirstOrDefaultAsync(p => p.InstanciaId == instanciaId && !p.IsDeleted);

        if (instancia == null || instancia.Plantilla == null)
            return null;

        var plantilla = instancia.Plantilla;
        var valores = instancia.Valores.ToDictionary(v => v.CampoId);

        return new PlantillaPOAViewModel
        {
            PlantillaId = plantilla.PlantillaId,
            ProgramaId = plantilla.ProgramaId,
            Version = plantilla.Version,
            Estado = plantilla.Estado.ToString(),
            VigenteDesde = plantilla.VigenteDesde,
            VigenteHasta = plantilla.VigenteHasta,
            Secciones = plantilla.Secciones
                .OrderBy(s => s.Orden)
                .Select(s => new SeccionPOAViewModel
                {
                    SeccionId = s.SeccionId,
                    Nombre = s.Nombre,
                    Orden = s.Orden,
                    Campos = plantilla.Campos
                        .Where(c => c.SeccionId == s.SeccionId)
                        .OrderBy(c => c.Orden)
                        .Select(c => new CampoPOAViewModel
                        {
                            CampoId = c.CampoId,
                            Clave = c.Clave,
                            Etiqueta = c.Etiqueta,
                            TipoDato = c.TipoDato.ToString(),
                            Requerido = c.Requerido,
                            Orden = c.Orden,
                            Unidad = c.Unidad,
                            Alcance = c.Alcance.ToString(),
                            Opciones = c.Opciones.Select(o => new OpcionCampoViewModel
                            {
                                OpcionId = o.OpcionId,
                                Valor = o.Valor,
                                Etiqueta = o.Etiqueta
                            }).ToList(),
                            Validaciones = c.Validaciones.Select(v => new ValidacionCampoViewModel
                            {
                                ValidacionId = v.ValidacionId,
                                Tipo = v.Tipo.ToString(),
                                Parametro = v.Parametro
                            }).ToList(),
                            ValorTexto = valores.ContainsKey(c.CampoId) ? valores[c.CampoId].ValorTexto : null,
                            ValorNumero = valores.ContainsKey(c.CampoId)
                                ? (valores[c.CampoId].ValorDecimal ?? valores[c.CampoId].ValorNumero)
                                : null,
                            ValorFecha = valores.ContainsKey(c.CampoId) ? valores[c.CampoId].ValorFecha : null,
                            ValorBool = valores.ContainsKey(c.CampoId) ? valores[c.CampoId].ValorBool : null
                        }).ToList()
                }).ToList()
        };
    }

    public async Task<int> CrearPOAAsync(CrearPOAModel model)
    {
        // Validar que no exista ya un POA para ese periodo
        var existe = await _context.POAInstancias
            .AnyAsync(p => p.ProgramaId == model.ProgramaId
                        && p.PeriodoAnio == model.PeriodoAnio
                        && p.PeriodoMes == model.PeriodoMes
                        && !p.IsDeleted);

        if (existe)
        {
            throw new InvalidOperationException("Ya existe un POA para este periodo");
        }

        // Obtener plantilla activa
        var plantilla = await _context.POAPlantillas
            .Include(p => p.Campos)
            .Where(p => p.ProgramaId == model.ProgramaId
                     && p.Estado == EstadoPlantilla.Activa
                     && !p.IsDeleted)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync();

        if (plantilla == null)
        {
            throw new InvalidOperationException("No hay plantilla activa para este programa");
        }

        var instancia = new POAInstancia
        {
            ProgramaId = model.ProgramaId,
            PlantillaId = plantilla.PlantillaId,
            PeriodoAnio = model.PeriodoAnio,
            PeriodoMes = model.PeriodoMes,
            Estado = EstadoInstancia.Borrador,
            Notas = model.Notas,
            CreadoEn = DateTime.UtcNow
        };

        _context.POAInstancias.Add(instancia);
        await _context.SaveChangesAsync();

        // Guardar TODOS los valores de los campos
        var valores = new List<POAValor>();

        // 1. Actividades Planificadas (SIEMPRE guardar, incluso si es 0)
        var campoActividadesPlan = plantilla.Campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("ACTIVIDADES_PLANIFICADAS") || 
            c.Clave.ToUpper().Contains("TALLERES_PLAN") || 
            c.Clave.ToUpper().Contains("SESIONES_PLAN"));
        if (campoActividadesPlan != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instancia.InstanciaId,
                CampoId = campoActividadesPlan.CampoId,
                ProgramaId = model.ProgramaId,
                ValorNumero = model.ActividadesPlanificadas,
                ValorDecimal = model.ActividadesPlanificadas,
                CreadoEn = DateTime.UtcNow
            });
        }

        // 2. Actividades Ejecutadas (SIEMPRE guardar, incluso si es 0)
        var campoActividadesEjec = plantilla.Campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("ACTIVIDADES_EJECUTADAS") || 
            c.Clave.ToUpper().Contains("TALLERES_EJEC") || 
            c.Clave.ToUpper().Contains("SESIONES_EJEC"));
        if (campoActividadesEjec != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instancia.InstanciaId,
                CampoId = campoActividadesEjec.CampoId,
                ProgramaId = model.ProgramaId,
                ValorNumero = model.ActividadesEjecutadas,
                ValorDecimal = model.ActividadesEjecutadas,
                CreadoEn = DateTime.UtcNow
            });
        }

        // 3. Presupuesto Total
        var campoPresupuestoTotal = plantilla.Campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("PRESUPUESTO_TOTAL") ||
            (c.Clave.ToUpper().Contains("PRESUPUESTO") && !c.Clave.ToUpper().Contains("EJECUTADO")) || 
            c.Clave.ToUpper().Contains("BUDGET"));
        if (campoPresupuestoTotal != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instancia.InstanciaId,
                CampoId = campoPresupuestoTotal.CampoId,
                ProgramaId = model.ProgramaId,
                ValorDecimal = model.PresupuestoTotal ?? 0,
                CreadoEn = DateTime.UtcNow
            });
        }

        // 4. Presupuesto Ejecutado
        var campoPresupuestoEjec = plantilla.Campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("PRESUPUESTO_EJECUTADO"));
        if (campoPresupuestoEjec != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instancia.InstanciaId,
                CampoId = campoPresupuestoEjec.CampoId,
                ProgramaId = model.ProgramaId,
                ValorDecimal = model.PresupuestoEjecutado ?? 0,
                CreadoEn = DateTime.UtcNow
            });
        }

        // 5. Total Participantes
        var campoParticipantes = plantilla.Campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("TOTAL_PARTICIPANTES") ||
            (c.Clave.ToUpper().Contains("PARTICIPANTES") && !c.Clave.ToUpper().Contains("ACTIVOS") && !c.Clave.ToUpper().Contains("NUEVOS") && !c.Clave.ToUpper().Contains("RETIRADOS")) || 
            c.Clave.ToUpper().Contains("BENEFICIARIOS"));
        if (campoParticipantes != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instancia.InstanciaId,
                CampoId = campoParticipantes.CampoId,
                ProgramaId = model.ProgramaId,
                ValorNumero = model.TotalParticipantes ?? 0,
                ValorDecimal = model.TotalParticipantes ?? 0,
                CreadoEn = DateTime.UtcNow
            });
        }

        // 6. Participantes Activos
        var campoParticipantesActivos = plantilla.Campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("PARTICIPANTES_ACTIVOS"));
        if (campoParticipantesActivos != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instancia.InstanciaId,
                CampoId = campoParticipantesActivos.CampoId,
                ProgramaId = model.ProgramaId,
                ValorNumero = model.ParticipantesActivos ?? 0,
                ValorDecimal = model.ParticipantesActivos ?? 0,
                CreadoEn = DateTime.UtcNow
            });
        }

        // 7. Nuevos Participantes
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "NUEVOS_PARTICIPANTES", model.NuevosParticipantes);

        // 8. Participantes Retirados
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "PARTICIPANTES_RETIRADOS", model.ParticipantesRetirados);

        // 9. Facilitadores
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "FACILITADORES", model.TotalFacilitadores);

        // 10. Voluntarios
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "VOLUNTARIOS", model.TotalVoluntarios, excluir: "HORAS");

        // 11. Horas Voluntariado
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "HORAS_VOLUNTARIADO", model.HorasVoluntariado);

        // 12. Objetivos Cumplidos
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "OBJETIVOS_CUMPLIDOS", model.ObjetivosCumplidos);

        // 13. Objetivos Totales
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "OBJETIVOS_TOTALES", model.ObjetivosTotales);

        // 14. Familias Impactadas
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "FAMILIAS_IMPACTADAS", model.FamiliasImpactadas);

        // 15. Comunidades Alcanzadas
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "COMUNIDADES_ALCANZADAS", model.ComunidadesAlcanzadas);

        // 16. Alianzas Activas
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "ALIANZAS_ACTIVAS", model.AlianzasActivas);

        // 17. Nuevas Alianzas
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "NUEVAS_ALIANZAS", model.NuevasAlianzas);

        // 18. Aportes en Especie
        GuardarValorDecimalSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "APORTES_ESPECIE", model.AportesEnEspecie);

        // 19. Capacitaciones Realizadas
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "CAPACITACIONES_REALIZADAS", model.CapacitacionesRealizadas);

        // 20. Personas Capacitadas
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "PERSONAS_CAPACITADAS", model.PersonasCapacitadas);

        // 21. Certificados Emitidos
        GuardarValorSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "CERTIFICADOS_EMITIDOS", model.CertificadosEmitidos);

        // 22. Logros Principales
        GuardarValorTextoSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "LOGROS_PRINCIPALES", model.LogrosPrincipales);

        // 23. Retos Enfrentados
        GuardarValorTextoSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "RETOS_ENFRENTADOS", model.RetosEnfrentados);

        // 24. Lecciones Aprendidas
        GuardarValorTextoSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "LECCIONES_APRENDIDAS", model.LeccionesAprendidas);

        // 25. Próximos Pasos
        GuardarValorTextoSiExiste(plantilla, valores, instancia.InstanciaId, model.ProgramaId,
            "PROXIMOS_PASOS", model.ProximosPasos);

        // Guardar todos los valores
        if (valores.Any())
        {
            _context.POAValores.AddRange(valores);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("POA creado: InstanciaId={InstanciaId}, Programa={ProgramaId}, Periodo={Anio}-{Mes}, Valores={CantidadValores}",
            instancia.InstanciaId, model.ProgramaId, model.PeriodoAnio, model.PeriodoMes, valores.Count);

        return instancia.InstanciaId;
    }

    private void GuardarValorSiExiste(POAPlantilla plantilla, List<POAValor> valores, int instanciaId, int programaId, 
        string claveBusqueda, int? valor, string? excluir = null)
    {
        if (!valor.HasValue) return;
        
        var campo = plantilla.Campos.FirstOrDefault(c =>
        {
            var clave = c.Clave.ToUpper();
            if (excluir != null && clave.Contains(excluir.ToUpper())) return false;
            return clave.Contains(claveBusqueda.ToUpper());
        });
        
        if (campo != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instanciaId,
                CampoId = campo.CampoId,
                ProgramaId = programaId,
                ValorNumero = valor,
                ValorDecimal = valor,
                CreadoEn = DateTime.UtcNow
            });
        }
    }

    private void GuardarValorDecimalSiExiste(POAPlantilla plantilla, List<POAValor> valores, int instanciaId, int programaId, 
        string claveBusqueda, decimal? valor)
    {
        if (!valor.HasValue) return;
        
        var campo = plantilla.Campos.FirstOrDefault(c => c.Clave.ToUpper().Contains(claveBusqueda.ToUpper()));
        
        if (campo != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instanciaId,
                CampoId = campo.CampoId,
                ProgramaId = programaId,
                ValorDecimal = valor,
                CreadoEn = DateTime.UtcNow
            });
        }
    }

    private void GuardarValorTextoSiExiste(POAPlantilla plantilla, List<POAValor> valores, int instanciaId, int programaId, 
        string claveBusqueda, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return;
        
        var campo = plantilla.Campos.FirstOrDefault(c => c.Clave.ToUpper().Contains(claveBusqueda.ToUpper()));
        
        if (campo != null)
        {
            valores.Add(new POAValor
            {
                InstanciaId = instanciaId,
                CampoId = campo.CampoId,
                ProgramaId = programaId,
                ValorTexto = valor,
                CreadoEn = DateTime.UtcNow
            });
        }
    }

    public async Task GuardarValoresAsync(int instanciaId, Dictionary<int, ValorCampoModel> valores)
    {
        var instancia = await _context.POAInstancias
            .Include(p => p.Valores)
            .FirstOrDefaultAsync(p => p.InstanciaId == instanciaId && !p.IsDeleted);

        if (instancia == null)
        {
            throw new InvalidOperationException("POA no encontrado");
        }

        foreach (var (campoId, valor) in valores)
        {
            var existente = instancia.Valores.FirstOrDefault(v => v.CampoId == campoId && !v.IsDeleted);

            if (existente != null)
            {
                existente.ValorTexto = valor.ValorTexto;
                existente.ValorNumero = valor.ValorNumero.HasValue ? (int?)valor.ValorNumero.Value : null;
                existente.ValorDecimal = valor.ValorNumero;
                existente.ValorFecha = valor.ValorFecha;
                existente.ValorBool = valor.ValorBool;
                existente.ActualizadoEn = DateTime.UtcNow;
            }
            else
            {
                instancia.Valores.Add(new POAValor
                {
                    InstanciaId = instanciaId,
                    CampoId = campoId,
                    ValorTexto = valor.ValorTexto,
                    ValorNumero = valor.ValorNumero.HasValue ? (int?)valor.ValorNumero.Value : null,
                    ValorDecimal = valor.ValorNumero,
                    ValorFecha = valor.ValorFecha,
                    ValorBool = valor.ValorBool,
                    CreadoEn = DateTime.UtcNow
                });
            }
        }

        instancia.ActualizadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Valores POA guardados: InstanciaId={InstanciaId}, Campos={CantidadCampos}",
            instanciaId, valores.Count);
    }

    public async Task<MetricasPOAViewModel> CalcularMetricasAsync(int instanciaId)
    {
        var valores = await _context.POAValores
            .Include(v => v.Campo)
            .Where(v => v.InstanciaId == instanciaId && !v.IsDeleted)
            .ToListAsync();

        var metricas = new MetricasPOAViewModel();

        foreach (var valor in valores)
        {
            var clave = valor.Campo.Clave.ToUpper();
            
            // Presupuesto
            if (clave.Contains("PRESUPUESTO_EJECUTADO"))
            {
                metricas.PresupuestoEjecutado = valor.ValorDecimal ?? valor.ValorNumero ?? 0;
                metricas.TotalEgresos = metricas.PresupuestoEjecutado;
            }
            else if (clave.Contains("PRESUPUESTO_TOTAL") || clave.Contains("PRESUPUESTO") || clave.Contains("BUDGET"))
            {
                metricas.PresupuestoTotal = valor.ValorDecimal ?? valor.ValorNumero ?? 0;
                metricas.TotalIngresos = metricas.PresupuestoTotal;
            }
            // Participantes
            else if (clave.Contains("PARTICIPANTES_ACTIVOS"))
            {
                metricas.ParticipantesActivos = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("NUEVOS_PARTICIPANTES"))
            {
                metricas.NuevosParticipantes = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("PARTICIPANTES_RETIRADOS"))
            {
                metricas.ParticipantesRetirados = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("TOTAL_PARTICIPANTES") || clave.Contains("PARTICIPANTES") || clave.Contains("BENEFICIARIOS"))
            {
                metricas.TotalParticipantes = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            // Actividades
            else if (clave.Contains("ACTIVIDADES_PLANIFICADAS") || clave.Contains("TALLERES_PLAN") || clave.Contains("SESIONES_PLAN"))
            {
                metricas.ActividadesPlanificadas = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("ACTIVIDADES_EJECUTADAS") || clave.Contains("TALLERES_EJEC") || clave.Contains("SESIONES_EJEC"))
            {
                metricas.ActividadesEjecutadas = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            // Recursos Humanos
            else if (clave.Contains("FACILITADORES"))
            {
                metricas.TotalFacilitadores = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("HORAS_VOLUNTARIADO"))
            {
                metricas.HorasVoluntariado = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("VOLUNTARIOS"))
            {
                metricas.TotalVoluntarios = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            // Impacto
            else if (clave.Contains("OBJETIVOS_CUMPLIDOS"))
            {
                metricas.ObjetivosCumplidos = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("OBJETIVOS_TOTALES") || clave.Contains("OBJETIVOS_PLANTEADOS"))
            {
                metricas.ObjetivosTotales = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("FAMILIAS_IMPACTADAS"))
            {
                metricas.FamiliasImpactadas = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("COMUNIDADES_ALCANZADAS"))
            {
                metricas.ComunidadesAlcanzadas = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            // Alianzas
            else if (clave.Contains("ALIANZAS_ACTIVAS") || clave.Contains("ALIANZAS"))
            {
                metricas.AlianzasActivas = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("APORTES_ESPECIE"))
            {
                metricas.AportesEnEspecie = valor.ValorDecimal ?? 0;
            }
            // Capacitaciones
            else if (clave.Contains("CAPACITACIONES_REALIZADAS"))
            {
                metricas.CapacitacionesRealizadas = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("PERSONAS_CAPACITADAS"))
            {
                metricas.PersonasCapacitadas = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            else if (clave.Contains("CERTIFICADOS_EMITIDOS"))
            {
                metricas.CertificadosEmitidos = (int)(valor.ValorDecimal ?? valor.ValorNumero ?? 0);
            }
            // Narrativa
            else if (clave.Contains("LOGROS_PRINCIPALES"))
            {
                metricas.LogrosPrincipales = valor.ValorTexto;
            }
            else if (clave.Contains("RETOS_ENFRENTADOS"))
            {
                metricas.RetosEnfrentados = valor.ValorTexto;
            }
            else if (clave.Contains("LECCIONES_APRENDIDAS"))
            {
                metricas.LeccionesAprendidas = valor.ValorTexto;
            }
            else if (clave.Contains("PROXIMOS_PASOS"))
            {
                metricas.ProximosPasos = valor.ValorTexto;
            }
        }

        // Generar métricas dinámicas para todos los campos numéricos
        metricas.MetricasDinamicas = valores
            .Where(v => (v.ValorDecimal.HasValue || v.ValorNumero.HasValue) &&
                       (v.Campo.TipoDato == TipoDato.Decimal || v.Campo.TipoDato == TipoDato.Entero))
            .Select(v => new MetricaDinamica
            {
                Etiqueta = v.Campo.Etiqueta,
                Valor = v.ValorDecimal ?? v.ValorNumero ?? 0,
                Unidad = v.Campo.Unidad
            })
            .ToList();

        _logger.LogInformation("Métricas calculadas: InstanciaId={InstanciaId}, Presupuesto={Presupuesto}, Participantes={Participantes}",
            instanciaId, metricas.PresupuestoTotal, metricas.TotalParticipantes);

        return metricas;
    }

    public async Task EliminarPOAAsync(int instanciaId)
    {
        var instancia = await _context.POAInstancias
            .FirstOrDefaultAsync(p => p.InstanciaId == instanciaId && !p.IsDeleted);

        if (instancia == null)
        {
            throw new InvalidOperationException("POA no encontrado");
        }

        instancia.IsDeleted = true;
        instancia.EliminadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("POA eliminado: InstanciaId={InstanciaId}", instanciaId);
    }

    public async Task ActualizarPOAAsync(EditarPOAModel model)
    {
        var instancia = await _context.POAInstancias
            .Include(p => p.Plantilla)
                .ThenInclude(pl => pl.Campos)
            .Include(p => p.Valores)
            .FirstOrDefaultAsync(p => p.InstanciaId == model.InstanciaId && !p.IsDeleted);

        if (instancia == null)
        {
            throw new InvalidOperationException("POA no encontrado");
        }

        // Actualizar notas
        instancia.Notas = model.Notas;
        instancia.ActualizadoEn = DateTime.UtcNow;

        // Buscar campos en la plantilla
        var campos = instancia.Plantilla.Campos.ToList();

        _logger.LogInformation("Actualizando POA {InstanciaId}. Campos en plantilla: {CantidadCampos}", 
            model.InstanciaId, campos.Count);

        // 1. Actividades Planificadas
        var campoActividadesPlan = campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("ACTIVIDADES_PLANIFICADAS") || 
            c.Clave.ToUpper().Contains("TALLERES_PLAN") || 
            c.Clave.ToUpper().Contains("SESIONES_PLAN"));
        if (campoActividadesPlan != null)
        {
            await ActualizarOCrearValorAsync(instancia, campoActividadesPlan.CampoId, model.ProgramaId, 
                numeroValor: model.ActividadesPlanificadas, nombreCampo: "Actividades Planificadas");
        }
        else
        {
            _logger.LogWarning("Campo 'Actividades Planificadas' no encontrado en plantilla");
        }

        // 2. Actividades Ejecutadas
        var campoActividadesEjec = campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("ACTIVIDADES_EJECUTADAS") || 
            c.Clave.ToUpper().Contains("TALLERES_EJEC") || 
            c.Clave.ToUpper().Contains("SESIONES_EJEC"));
        if (campoActividadesEjec != null)
        {
            await ActualizarOCrearValorAsync(instancia, campoActividadesEjec.CampoId, model.ProgramaId, 
                numeroValor: model.ActividadesEjecutadas, nombreCampo: "Actividades Ejecutadas");
        }
        else
        {
            _logger.LogWarning("Campo 'Actividades Ejecutadas' no encontrado en plantilla");
        }

        // 3. Presupuesto Total
        var campoPresupuestoTotal = campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("PRESUPUESTO_TOTAL") || 
            (c.Clave.ToUpper().Contains("PRESUPUESTO") && !c.Clave.ToUpper().Contains("EJECUTADO")) || 
            c.Clave.ToUpper().Contains("BUDGET"));
        if (campoPresupuestoTotal != null)
        {
            await ActualizarOCrearValorAsync(instancia, campoPresupuestoTotal.CampoId, model.ProgramaId, 
                decimalValor: model.PresupuestoTotal ?? 0, nombreCampo: "Presupuesto Total");
        }
        else
        {
            _logger.LogWarning("Campo 'Presupuesto Total' no encontrado en plantilla. Claves disponibles: {Claves}", 
                string.Join(", ", campos.Select(c => c.Clave)));
        }

        // 4. Presupuesto Ejecutado
        var campoPresupuestoEjec = campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("PRESUPUESTO_EJECUTADO"));
        if (campoPresupuestoEjec != null)
        {
            await ActualizarOCrearValorAsync(instancia, campoPresupuestoEjec.CampoId, model.ProgramaId, 
                decimalValor: model.PresupuestoEjecutado ?? 0, nombreCampo: "Presupuesto Ejecutado");
        }
        else
        {
            _logger.LogWarning("Campo 'Presupuesto Ejecutado' no encontrado en plantilla");
        }

        // 5. Total Participantes
        var campoParticipantes = campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("TOTAL_PARTICIPANTES") ||
            (c.Clave.ToUpper().Contains("PARTICIPANTES") && !c.Clave.ToUpper().Contains("ACTIVOS")) || 
            c.Clave.ToUpper().Contains("BENEFICIARIOS"));
        if (campoParticipantes != null)
        {
            await ActualizarOCrearValorAsync(instancia, campoParticipantes.CampoId, model.ProgramaId, 
                numeroValor: model.TotalParticipantes ?? 0, nombreCampo: "Total Participantes");
        }
        else
        {
            _logger.LogWarning("Campo 'Total Participantes' no encontrado en plantilla");
        }

        // 6. Participantes Activos
        var campoParticipantesActivos = campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("PARTICIPANTES_ACTIVOS"));
        if (campoParticipantesActivos != null)
        {
            await ActualizarOCrearValorAsync(instancia, campoParticipantesActivos.CampoId, model.ProgramaId, 
                numeroValor: model.ParticipantesActivos ?? 0, nombreCampo: "Participantes Activos");
        }
        else
        {
            _logger.LogWarning("Campo 'Participantes Activos' no encontrado en plantilla");
        }

        // 7. Nuevos Participantes
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "NUEVOS_PARTICIPANTES", model.NuevosParticipantes, "Nuevos Participantes");

        // 8. Participantes Retirados
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "PARTICIPANTES_RETIRADOS", model.ParticipantesRetirados, "Participantes Retirados");

        // 9. Facilitadores
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "FACILITADORES", model.TotalFacilitadores, "Facilitadores");

        // 10. Voluntarios
        var campoVoluntarios = campos.FirstOrDefault(c => 
            c.Clave.ToUpper().Contains("VOLUNTARIOS") && !c.Clave.ToUpper().Contains("HORAS"));
        if (campoVoluntarios != null && model.TotalVoluntarios.HasValue)
        {
            await ActualizarOCrearValorAsync(instancia, campoVoluntarios.CampoId, model.ProgramaId, 
                numeroValor: model.TotalVoluntarios, nombreCampo: "Voluntarios");
        }

        // 11. Horas Voluntariado
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "HORAS_VOLUNTARIADO", model.HorasVoluntariado, "Horas Voluntariado");

        // 12. Objetivos Cumplidos
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "OBJETIVOS_CUMPLIDOS", model.ObjetivosCumplidos, "Objetivos Cumplidos");

        // 13. Objetivos Totales
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "OBJETIVOS_TOTALES", model.ObjetivosTotales, "Objetivos Totales");

        // 14. Familias Impactadas
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "FAMILIAS_IMPACTADAS", model.FamiliasImpactadas, "Familias Impactadas");

        // 15. Comunidades Alcanzadas
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "COMUNIDADES_ALCANZADAS", model.ComunidadesAlcanzadas, "Comunidades Alcanzadas");

        // 16. Alianzas Activas
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "ALIANZAS_ACTIVAS", model.AlianzasActivas, "Alianzas Activas");

        // 17. Nuevas Alianzas
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "NUEVAS_ALIANZAS", model.NuevasAlianzas, "Nuevas Alianzas");

        // 18. Aportes en Especie
        await ActualizarCampoDecimalSiExisteAsync(instancia, campos, model.ProgramaId, 
            "APORTES_ESPECIE", model.AportesEnEspecie, "Aportes en Especie");

        // 19. Capacitaciones Realizadas
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "CAPACITACIONES_REALIZADAS", model.CapacitacionesRealizadas, "Capacitaciones Realizadas");

        // 20. Personas Capacitadas
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "PERSONAS_CAPACITADAS", model.PersonasCapacitadas, "Personas Capacitadas");

        // 21. Certificados Emitidos
        await ActualizarCampoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "CERTIFICADOS_EMITIDOS", model.CertificadosEmitidos, "Certificados Emitidos");

        // 22. Logros Principales
        await ActualizarCampoTextoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "LOGROS_PRINCIPALES", model.LogrosPrincipales, "Logros Principales");

        // 23. Retos Enfrentados
        await ActualizarCampoTextoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "RETOS_ENFRENTADOS", model.RetosEnfrentados, "Retos Enfrentados");

        // 24. Lecciones Aprendidas
        await ActualizarCampoTextoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "LECCIONES_APRENDIDAS", model.LeccionesAprendidas, "Lecciones Aprendidas");

        // 25. Próximos Pasos
        await ActualizarCampoTextoSiExisteAsync(instancia, campos, model.ProgramaId, 
            "PROXIMOS_PASOS", model.ProximosPasos, "Próximos Pasos");

        // GUARDAR CAMBIOS
        var cambios = await _context.SaveChangesAsync();

        _logger.LogInformation("POA actualizado: InstanciaId={InstanciaId}, CambiosGuardados={Cambios}",
            model.InstanciaId, cambios);
    }

    private async Task ActualizarCampoSiExisteAsync(POAInstancia instancia, List<POACampo> campos, int programaId,
        string claveBusqueda, int? valor, string nombreCampo)
    {
        var campo = campos.FirstOrDefault(c => c.Clave.ToUpper().Contains(claveBusqueda.ToUpper()));
        if (campo != null && valor.HasValue)
        {
            await ActualizarOCrearValorAsync(instancia, campo.CampoId, programaId, 
                numeroValor: valor, nombreCampo: nombreCampo);
        }
    }

    private async Task ActualizarCampoDecimalSiExisteAsync(POAInstancia instancia, List<POACampo> campos, int programaId,
        string claveBusqueda, decimal? valor, string nombreCampo)
    {
        var campo = campos.FirstOrDefault(c => c.Clave.ToUpper().Contains(claveBusqueda.ToUpper()));
        if (campo != null && valor.HasValue)
        {
            await ActualizarOCrearValorAsync(instancia, campo.CampoId, programaId, 
                decimalValor: valor, nombreCampo: nombreCampo);
        }
    }

    private async Task ActualizarCampoTextoSiExisteAsync(POAInstancia instancia, List<POACampo> campos, int programaId,
        string claveBusqueda, string? valor, string nombreCampo)
    {
        var campo = campos.FirstOrDefault(c => c.Clave.ToUpper().Contains(claveBusqueda.ToUpper()));
        if (campo != null && !string.IsNullOrWhiteSpace(valor))
        {
            await ActualizarOCrearValorAsync(instancia, campo.CampoId, programaId, 
                textoValor: valor, nombreCampo: nombreCampo);
        }
    }

    private async Task ActualizarOCrearValorAsync(POAInstancia instancia, int campoId, int programaId, 
        int? numeroValor = null, decimal? decimalValor = null, string? textoValor = null, string? nombreCampo = null)
    {
        // Buscar el valor existente directamente en la base de datos para asegurar tracking
        var valor = await _context.POAValores
            .FirstOrDefaultAsync(v => v.InstanciaId == instancia.InstanciaId 
                && v.CampoId == campoId 
                && !v.IsDeleted);

        if (valor != null)
        {
            // Actualizar valor existente
            var valorAnterior = valor.ValorDecimal ?? valor.ValorNumero;
            
            valor.ValorNumero = numeroValor;
            valor.ValorDecimal = decimalValor ?? (numeroValor.HasValue ? (decimal)numeroValor.Value : (decimal?)null);
            valor.ValorTexto = textoValor;
            valor.ActualizadoEn = DateTime.UtcNow;
            
            // Marcar explícitamente como modificado
            _context.Entry(valor).State = EntityState.Modified;
            
            _logger.LogInformation("✏️ Campo '{Campo}' actualizado: {ValorAnterior} → {ValorNuevo}", 
                nombreCampo ?? $"CampoId_{campoId}", valorAnterior, decimalValor ?? numeroValor);
        }
        else
        {
            // Crear nuevo valor
            var nuevoValor = new POAValor
            {
                InstanciaId = instancia.InstanciaId,
                CampoId = campoId,
                ProgramaId = programaId,
                ValorNumero = numeroValor,
                ValorDecimal = decimalValor ?? (numeroValor.HasValue ? (decimal)numeroValor.Value : (decimal?)null),
                ValorTexto = textoValor,
                CreadoEn = DateTime.UtcNow
            };
            
            _context.POAValores.Add(nuevoValor);
            
            _logger.LogInformation("➕ Campo '{Campo}' creado: {Valor}", 
                nombreCampo ?? $"CampoId_{campoId}", decimalValor ?? numeroValor);
        }
    }

    public async Task AprobarPOAAsync(int instanciaId)
    {
        var instancia = await _context.POAInstancias
            .FirstOrDefaultAsync(p => p.InstanciaId == instanciaId && !p.IsDeleted);

        if (instancia == null)
        {
            throw new InvalidOperationException("POA no encontrado");
        }

        instancia.Estado = EstadoInstancia.Aprobado;
        instancia.ActualizadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("POA aprobado: InstanciaId={InstanciaId}", instanciaId);
    }
}
