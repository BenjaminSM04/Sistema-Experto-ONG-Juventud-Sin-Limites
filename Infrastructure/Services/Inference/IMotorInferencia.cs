namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference
{
    public interface IMotorInferencia
    {
        Task<EjecucionResumen> EjecutarAsync(DateOnly fechaCorte, int? programaId, bool dryRun, CancellationToken ct);
    }
    public record EjecucionResumen(int ReglasEjecutadas, int AlertasGeneradas, int Errores);
}
