namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference
{
    public interface IFeatureProvider
    {
        Task<double> PorcAsistenciaParticipanteAsync(int participanteId, int programaId, DateOnly desde, DateOnly hasta, CancellationToken ct);

        Task<int> ConsecutivasAusenciasAsync(int participanteId, int actividadId, DateOnly hasta, CancellationToken ct);

        Task<decimal?> PoaDecimalAsync(int instanciaId, string campoClave, int? programaId, int? actividadId, int? participanteId, CancellationToken ct);

        Task<(int plan, int ejec)> PlanVsEjecAsync(int programaId, string anioMes, CancellationToken ct);
    }
}
