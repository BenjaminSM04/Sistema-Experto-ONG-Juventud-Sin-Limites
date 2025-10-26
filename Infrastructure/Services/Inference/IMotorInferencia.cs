namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference
{
    public interface IMotorInferencia
    {
        Task<EjecucionResumen> EjecutarAsync(DateOnly fechaCorte, int? programaId, CancellationToken ct);
    }
    public record EjecucionResumen(int ReglasEjecutadas, int AlertasGeneradas, int Errores);
}
