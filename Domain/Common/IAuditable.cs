namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

public interface IAuditable
{
    DateTime CreadoEn { get; set; }
    int? CreadoPorUsuarioId { get; set; }
    DateTime? ActualizadoEn { get; set; }
    int? ActualizadoPorUsuarioId { get; set; }
}
