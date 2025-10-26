using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference
{
    public class FeatureProvider : IFeatureProvider
    {
        private readonly ApplicationDbContext _db;
        public FeatureProvider(ApplicationDbContext db) => _db = db;

        public async Task<double> PorcAsistenciaParticipanteAsync(int participanteId, int programaId, DateOnly desde, DateOnly hasta, CancellationToken ct)
        {
            var q = from a in _db.Asistencias
                    join act in _db.Actividades on a.ActividadId equals act.ActividadId
                    where a.ParticipanteId == participanteId
                      && act.ProgramaId == programaId
                      && a.Fecha >= desde.ToDateTime(TimeOnly.MinValue)
                      && a.Fecha <= hasta.ToDateTime(TimeOnly.MaxValue)
                      && !a.IsDeleted && !act.IsDeleted
                    select a.Estado; // 1=Presente, 2=Ausente, 3=Tarde, 4=Justificado

            var list = await q.ToListAsync(ct);
            if (list.Count == 0) return 0d;
            var presentes = list.Count(x => x == EstadoAsistencia.Presente || x == EstadoAsistencia.Tarde); // cuenta tarde como presente si quieres
            return (100.0 * presentes) / list.Count;
        }

        public async Task<int> ConsecutivasAusenciasAsync(int participanteId, int actividadId, DateOnly hasta, CancellationToken ct)
        {
            var fechas = await _db.Asistencias
                .Where(x => x.ParticipanteId == participanteId && x.ActividadId == actividadId
                            && x.Fecha <= hasta.ToDateTime(TimeOnly.MaxValue) && !x.IsDeleted)
                .OrderByDescending(x => x.Fecha)
                .Select(x => new { x.Fecha, x.Estado })
                .ToListAsync(ct);

            int streak = 0;
            foreach (var r in fechas)
            {
                if (r.Estado == EstadoAsistencia.Ausente) streak++;     
                else break;                      
            }
            return streak;
        }

        public async Task<decimal?> PoaDecimalAsync(int instanciaId, string campoClave, int? programaId, int? actividadId, int? participanteId, CancellationToken ct)
        {
            var q = from v in _db.POAValores
                    join c in _db.POACampos on v.CampoId equals c.CampoId
                    where v.InstanciaId == instanciaId && c.Clave == campoClave && !v.IsDeleted && !c.IsDeleted
                    select v;
            if (programaId.HasValue) q = q.Where(v => v.ProgramaId == programaId);
            if (actividadId.HasValue) q = q.Where(v => v.ActividadId == actividadId);
            if (participanteId.HasValue) q = q.Where(v => v.ParticipanteId == participanteId);

            var val = await q.Select(x => x.ValorDecimal).FirstOrDefaultAsync(ct);
            return val;
        }

        public async Task<(int plan, int ejec)> PlanVsEjecAsync(int programaId, string anioMes, CancellationToken ct)
        {
            var m = await _db.MetricasProgramaMes
                .FirstOrDefaultAsync(x => x.ProgramaId == programaId && x.AnioMes == anioMes && !x.IsDeleted, ct);
            return m is null ? (0, 0) : (m.ActividadesPlanificadas, m.ActividadesEjecutadas);
        }
    }
}
