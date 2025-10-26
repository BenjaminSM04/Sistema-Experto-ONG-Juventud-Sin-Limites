using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference
{
    public class MotorInferencia : IMotorInferencia
    {
        private readonly ApplicationDbContext _db;
        private readonly IFeatureProvider _features;

        public MotorInferencia(ApplicationDbContext db, IFeatureProvider features)
        {
            _db = db;
            _features = features;
        }

        public async Task<EjecucionResumen> EjecutarAsync(DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            var inicio = DateTime.UtcNow;
            var ejec = new EjecucionMotor
            {
                InicioUtc = inicio,
                Ambito = programaId.HasValue ? AmbitoEjecucion.EDV : AmbitoEjecucion.Todos,
                ResultadoResumen = ""
            };
            _db.EjecucionesMotor.Add(ejec);
            await _db.SaveChangesAsync(ct);

            int reglas = 0, alertas = 0, errores = 0;

            // 1) Carga reglas activas
            var qReglas = _db.Reglas.Where(r => r.Activa && !r.IsDeleted);
            var lista = await qReglas.ToListAsync(ct);

            foreach (var r in lista)
            {
                reglas++;
                try
                {
                    // 2) Merge parámetros (global + override)
                    var pars = await _db.ReglaParametros.Where(p => p.ReglaId == r.ReglaId && !p.IsDeleted).ToListAsync(ct);
                    var overrides = programaId.HasValue
                    ? await _db.ReglaParametroOverrides.Where(o => o.ReglaId == r.ReglaId && o.ProgramaId == programaId.Value && !o.IsDeleted).ToListAsync(ct)
                      : new List<ReglaParametroOverride>();

                    var dict = pars.ToDictionary(p => p.Nombre, p => (tipo: p.Tipo, val: p.Valor));
                    foreach (var ov in overrides) dict[ov.Nombre] = (ov.Tipo, ov.Valor);

                    // 3) Ejecuta por objetivo
                    switch (r.Objetivo)
                    {
                        case ObjetivoRegla.Participante:
                            alertas += await EjecutarParaParticipantes(r, dict, fechaCorte, programaId, ct);
                            break;
                        case ObjetivoRegla.Actividad:
                            alertas += await EjecutarParaActividades(r, dict, fechaCorte, programaId, ct);
                            break;
                        case ObjetivoRegla.Programa:
                            alertas += await EjecutarParaPrograma(r, dict, fechaCorte, programaId, ct);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errores++;
                    // Log the error (opcional: agregar logging)
                    Console.WriteLine($"Error ejecutando regla {r.Clave}: {ex.Message}");
                }
            }

            ejec.FinUtc = DateTime.UtcNow;
            ejec.Exitos = alertas;
            ejec.Errores = errores;
            ejec.ResultadoResumen = $"Reglas:{reglas}; Alertas:{alertas}; Errores:{errores}";
            await _db.SaveChangesAsync(ct);

            return new EjecucionResumen(reglas, alertas, errores);
        }

        // ========================================
        // REGLAS DE PARTICIPANTE
        // ========================================

        private async Task<int> EjecutarParaParticipantes(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            // REGLA 1: INASISTENCIA_CONSECUTIVA
            if (r.Clave == "INASISTENCIA_CONSECUTIVA")
            {
                generadas += await ReglaInasistenciaConsecutiva(r, p, fechaCorte, programaId, ct);
            }

            // REGLA 2: BAJA_ASISTENCIA_GENERAL
            if (r.Clave == "BAJA_ASISTENCIA_GENERAL")
            {
                generadas += await ReglaBajaAsistenciaGeneral(r, p, fechaCorte, programaId, ct);
            }

            return generadas;
        }

        /// <summary>
        /// Detecta participantes con múltiples inasistencias consecutivas en una actividad
        /// </summary>
        private async Task<int> ReglaInasistenciaConsecutiva(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            if (!p.ContainsKey("UMBRAL_AUSENCIAS"))
                return 0;

            int umbral = int.Parse(p["UMBRAL_AUSENCIAS"].val);

            // Obtener actividades del ámbito
            var acts = _db.Actividades
                .Where(a => !a.IsDeleted && (!programaId.HasValue || a.ProgramaId == programaId.Value));

            var inscripciones = from ap in _db.ActividadParticipantes
                                join a in acts on ap.ActividadId equals a.ActividadId
                                where !ap.IsDeleted
                                select new { ap.ParticipanteId, ap.ActividadId, a.ProgramaId };

            var lista = await inscripciones.ToListAsync(ct);

            foreach (var inscripcion in lista)
            {
                var streak = await _features.ConsecutivasAusenciasAsync(
                    inscripcion.ParticipanteId,
                    inscripcion.ActividadId,
                    fechaCorte,
                    ct);

                if (streak >= umbral)
                {
                    _db.Alertas.Add(new Alerta
                    {
                        ReglaId = r.ReglaId,
                        Severidad = r.Severidad,
                        Mensaje = $"Participante con {streak} inasistencias consecutivas (umbral: {umbral})",
                        ProgramaId = inscripcion.ProgramaId,
                        ActividadId = inscripcion.ActividadId,
                        ParticipanteId = inscripcion.ParticipanteId,
                        GeneradaEn = DateTime.UtcNow,
                        Estado = EstadoAlerta.Abierta
                    });
                    generadas++;
                }
            }

            if (generadas > 0)
                await _db.SaveChangesAsync(ct);

            return generadas;
        }

        /// <summary>
        /// Detecta participantes con porcentaje de asistencia por debajo del umbral
        /// </summary>
        private async Task<int> ReglaBajaAsistenciaGeneral(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            if (!p.ContainsKey("UMBRAL_PCT"))
                return 0;

            decimal umbralPct = decimal.Parse(p["UMBRAL_PCT"].val);

            // Obtener participantes del ámbito
            var participantes = await _db.Participantes
                .Where(pa => !pa.IsDeleted && pa.Estado == EstadoGeneral.Activo)
                .ToListAsync(ct);

            // Evaluar cada participante
            foreach (var participante in participantes)
            {
                // Calcular asistencia en el periodo (últimos 30 días desde fecha de corte)
                var desde = fechaCorte.AddDays(-30);

                if (!programaId.HasValue)
                {
                    // Evaluar todos los programas
                    var programasIds = await _db.Programas
                        .Where(pr => !pr.IsDeleted && pr.InferenciaActiva)
                        .Select(pr => pr.ProgramaId)
                        .ToListAsync(ct);

                    foreach (var progId in programasIds)
                    {
                        var porcAsistencia = await _features.PorcAsistenciaParticipanteAsync(
                            participante.ParticipanteId,
                            progId,
                            desde,
                            fechaCorte,
                            ct);

                        if (porcAsistencia < (double)umbralPct && porcAsistencia > 0) // Solo alertar si tiene asistencias registradas
                        {
                            _db.Alertas.Add(new Alerta
                            {
                                ReglaId = r.ReglaId,
                                Severidad = r.Severidad,
                                Mensaje = $"Asistencia {porcAsistencia:N1}% por debajo del umbral {umbralPct}%",
                                ProgramaId = progId,
                                ParticipanteId = participante.ParticipanteId,
                                GeneradaEn = DateTime.UtcNow,
                                Estado = EstadoAlerta.Abierta
                            });
                            generadas++;
                        }
                    }
                }
                else
                {
                    // Evaluar solo el programa especificado
                    var porcAsistencia = await _features.PorcAsistenciaParticipanteAsync(
                        participante.ParticipanteId,
                        programaId.Value,
                        desde,
                        fechaCorte,
                        ct);

                    if (porcAsistencia < (double)umbralPct && porcAsistencia > 0)
                    {
                        _db.Alertas.Add(new Alerta
                        {
                            ReglaId = r.ReglaId,
                            Severidad = r.Severidad,
                            Mensaje = $"Asistencia {porcAsistencia:N1}% por debajo del umbral {umbralPct}%",
                            ProgramaId = programaId.Value,
                            ParticipanteId = participante.ParticipanteId,
                            GeneradaEn = DateTime.UtcNow,
                            Estado = EstadoAlerta.Abierta
                        });
                        generadas++;
                    }
                }
            }

            if (generadas > 0)
                await _db.SaveChangesAsync(ct);

            return generadas;
        }

        // ========================================
        // REGLAS DE ACTIVIDAD
        // ========================================

        private async Task<int> EjecutarParaActividades(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            // REGLA 3: ACTIVIDAD_SIN_ASISTENTES
            if (r.Clave == "ACTIVIDAD_SIN_ASISTENTES")
            {
                generadas += await ReglaActividadSinAsistentes(r, p, fechaCorte, programaId, ct);
            }

            // REGLA 4: RETRASO_ACTIVIDAD
            if (r.Clave == "RETRASO_ACTIVIDAD")
            {
                generadas += await ReglaRetrasoActividad(r, p, fechaCorte, programaId, ct);
            }

            return generadas;
        }

        /// <summary>
        /// Detecta actividades planificadas sin participantes inscritos
        /// </summary>
        private async Task<int> ReglaActividadSinAsistentes(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            if (!p.ContainsKey("MIN_INSCRITOS"))
                return 0;

            int minInscritos = int.Parse(p["MIN_INSCRITOS"].val);

            // Obtener actividades planificadas
            var actividades = await _db.Actividades
                .Where(a => !a.IsDeleted
                            && a.Estado == EstadoActividad.Planificada
                            && (!programaId.HasValue || a.ProgramaId == programaId.Value))
                .Select(a => new { a.ActividadId, a.ProgramaId, a.Titulo })
                .ToListAsync(ct);

            foreach (var actividad in actividades)
            {
                var countInscritos = await _db.ActividadParticipantes
                    .Where(ap => ap.ActividadId == actividad.ActividadId
                                  && !ap.IsDeleted
                                  && ap.Estado == EstadoInscripcion.Inscrito)
                    .CountAsync(ct);

                if (countInscritos < minInscritos)
                {
                    _db.Alertas.Add(new Alerta
                    {
                        ReglaId = r.ReglaId,
                        Severidad = r.Severidad,
                        Mensaje = $"Actividad '{actividad.Titulo}' con {countInscritos} inscritos (mínimo: {minInscritos})",
                        ProgramaId = actividad.ProgramaId,
                        ActividadId = actividad.ActividadId,
                        GeneradaEn = DateTime.UtcNow,
                        Estado = EstadoAlerta.Abierta
                    });
                    generadas++;
                }
            }

            if (generadas > 0)
                await _db.SaveChangesAsync(ct);

            return generadas;
        }

        /// <summary>
        /// Detecta actividades planificadas que no se han ejecutado pasada su fecha
        /// </summary>
        private async Task<int> ReglaRetrasoActividad(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            if (!p.ContainsKey("DIAS_RETRASO"))
                return 0;

            int diasRetraso = int.Parse(p["DIAS_RETRASO"].val);
            var fechaLimite = fechaCorte.ToDateTime(TimeOnly.MinValue).AddDays(-diasRetraso);

            // Actividades planificadas cuya fecha de inicio ya pasó
            var actividadesRetrasadas = await _db.Actividades
                .Where(a => !a.IsDeleted
                            && a.Estado == EstadoActividad.Planificada
                            && a.FechaInicio < fechaLimite
                            && (!programaId.HasValue || a.ProgramaId == programaId.Value))
                .ToListAsync(ct);

            foreach (var actividad in actividadesRetrasadas)
            {
                var diasAtraso = (DateTime.Now - actividad.FechaInicio).Days;

                _db.Alertas.Add(new Alerta
                {
                    ReglaId = r.ReglaId,
                    Severidad = r.Severidad,
                    Mensaje = $"Actividad '{actividad.Titulo}' con {diasAtraso} días de retraso (fecha: {actividad.FechaInicio:dd/MM/yyyy})",
                    ProgramaId = actividad.ProgramaId,
                    ActividadId = actividad.ActividadId,
                    GeneradaEn = DateTime.UtcNow,
                    Estado = EstadoAlerta.Abierta
                });
                generadas++;
            }

            if (generadas > 0)
                await _db.SaveChangesAsync(ct);

            return generadas;
        }

        // ========================================
        // REGLAS DE PROGRAMA
        // ========================================

        private async Task<int> EjecutarParaPrograma(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            // REGLA 5: BAJO_CUMPLIMIENTO_POA
            if (r.Clave == "BAJO_CUMPLIMIENTO_POA")
            {
                generadas += await ReglaBajoCumplimientoPOA(r, p, fechaCorte, programaId, ct);
            }

            return generadas;
        }

        /// <summary>
        /// Detecta programas con desvío plan vs ejecución por encima del umbral
        /// </summary>
        private async Task<int> ReglaBajoCumplimientoPOA(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            if (!p.ContainsKey("UMBRAL_PCT"))
                return 0;

            decimal umbralPct = decimal.Parse(p["UMBRAL_PCT"].val);
            var anioMes = $"{fechaCorte.Year:D4}-{fechaCorte.Month:D2}";

            // Obtener programas a evaluar
            var programas = _db.Programas
                .Where(pr => !pr.IsDeleted
                             && pr.InferenciaActiva
                             && (!programaId.HasValue || pr.ProgramaId == programaId.Value));

            var programaIds = await programas.Select(pr => pr.ProgramaId).ToListAsync(ct);

            foreach (var progId in programaIds)
            {
                var (plan, ejec) = await _features.PlanVsEjecAsync(progId, anioMes, ct);

                if (plan <= 0)
                    continue; // No hay plan definido

                var gap = (plan - ejec) * 100m / plan;

                if (gap >= umbralPct)
                {
                    _db.Alertas.Add(new Alerta
                    {
                        ReglaId = r.ReglaId,
                        Severidad = r.Severidad,
                        Mensaje = $"Desvío plan vs ejecución: {gap:N1}% (umbral: {umbralPct}%). Plan: {plan}, Ejecutado: {ejec}",
                        ProgramaId = progId,
                        GeneradaEn = DateTime.UtcNow,
                        Estado = EstadoAlerta.Abierta
                    });
                    generadas++;
                }
            }

            if (generadas > 0)
                await _db.SaveChangesAsync(ct);

            return generadas;
        }
    }
}
