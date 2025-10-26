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
                catch
                {
                    errores++;
                }
            }

            ejec.FinUtc = DateTime.UtcNow;
            ejec.ResultadoResumen = $"Reglas:{reglas}; Alertas:{alertas}; Errores:{errores}";
            await _db.SaveChangesAsync(ct);

            return new EjecucionResumen(reglas, alertas, errores);
        }

        // === Reglas ejemplo (MVP) ===

        private async Task<int> EjecutarParaParticipantes(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            // Ejemplo: INASISTENCIAS_3X => 3 ausencias consecutivas en una actividad
            if (r.Clave == "INASISTENCIAS_3X")
            {
                int umbral = int.Parse(p["UMBRAL"].val); // p.ej., 3

                // Participantes/actividades en el ámbito (opcional: filtra por programa)
                var acts = _db.Actividades.Where(a => !a.IsDeleted && (!programaId.HasValue || a.ProgramaId == programaId.Value));
                var insc = from ap in _db.ActividadParticipantes
                           join a in acts on ap.ActividadId equals a.ActividadId
                           select new { ap.ParticipanteId, ap.ActividadId, a.ProgramaId };

                var lista = await insc.ToListAsync(ct);

                foreach (var x in lista)
                {
                    var streak = await _features.ConsecutivasAusenciasAsync(x.ParticipanteId, x.ActividadId, fechaCorte, ct);
                    if (streak >= umbral)
                    {
                        _db.Alertas.Add(new Alerta
                        {
                            ReglaId = r.ReglaId,
                            Severidad = r.Severidad,
                            Mensaje = $"Ausencias consecutivas: {streak} (≥ {umbral})",
                            ProgramaId = x.ProgramaId,
                            ActividadId = x.ActividadId,
                            ParticipanteId = x.ParticipanteId,
                            GeneradaEn = DateTime.UtcNow,
                            Estado = EstadoAlerta.Abierta
                        });
                        generadas++;
                    }
                }
                await _db.SaveChangesAsync(ct);
            }

            // Agrega aquí otras reglas de participante…

            return generadas;
        }

        private async Task<int> EjecutarParaActividades(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            // Ejemplos futuros…
            return 0;
        }

        private async Task<int> EjecutarParaPrograma(Regla r, IDictionary<string, (TipoParametro tipo, string val)> p, DateOnly fechaCorte, int? programaId, CancellationToken ct)
        {
            int generadas = 0;

            // Ejemplo: GAP_PLAN_EJEC >= % (alerta por desvío mensual)
            if (r.Clave == "GAP_PLAN_EJEC")
            {
                var umbralPct = decimal.Parse(p["UMBRAL_PCT"].val); // ej. 20
                var anioMes = $"{fechaCorte.Year:D4}-{fechaCorte.Month:D2}";

                var progs = _db.Programas.Where(x => !x.IsDeleted && (!programaId.HasValue || x.ProgramaId == programaId.Value));
                var ids = await progs.Select(x => x.ProgramaId).ToListAsync(ct);

                foreach (var pid in ids)
                {
                    var (plan, ejec) = await _features.PlanVsEjecAsync(pid, anioMes, ct);
                    if (plan <= 0) continue;
                    var gap = (plan - ejec) * 100m / plan;
                    if (gap >= umbralPct)
                    {
                        _db.Alertas.Add(new Alerta
                        {
                            ReglaId = r.ReglaId,
                            Severidad = r.Severidad,
                            Mensaje = $"Desvío plan vs ejecución {gap:N1}% (umbral {umbralPct}%)",
                            ProgramaId = pid,
                            GeneradaEn = DateTime.UtcNow,
                            Estado = EstadoAlerta.Abierta
                        });
                        generadas++;
                    }
                }
                await _db.SaveChangesAsync(ct);
            }

            return generadas;
        }
    }
}
