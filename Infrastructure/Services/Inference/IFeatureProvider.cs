namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference
{
    /// <summary>
    /// Proveedor de características/datos para el motor de inferencia
    /// </summary>
    public interface IFeatureProvider
    {
        /// <summary>
        /// Calcula el porcentaje de asistencia de un participante en un rango de fechas
        /// </summary>
        Task<double> PorcAsistenciaParticipanteAsync(int participanteId, int programaId, DateOnly desde, DateOnly hasta, CancellationToken ct);

        /// <summary>
        /// Cuenta ausencias consecutivas de un participante en una actividad
        /// </summary>
        Task<int> ConsecutivasAusenciasAsync(int participanteId, int actividadId, DateOnly hasta, CancellationToken ct);

        /// <summary>
        /// Obtiene un valor decimal de un campo del POA
        /// </summary>
        Task<decimal?> PoaDecimalAsync(int instanciaId, string campoClave, int? programaId, int? actividadId, int? participanteId, CancellationToken ct);

        /// <summary>
        /// Obtiene plan vs ejecución de un programa en un mes específico
        /// </summary>
        Task<(int plan, int ejec)> PlanVsEjecAsync(int programaId, string anioMes, CancellationToken ct);
    }
}
