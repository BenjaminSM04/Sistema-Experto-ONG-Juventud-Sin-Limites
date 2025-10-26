namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

public abstract class BaseEntity : IAuditable, ISoftDelete
{
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public int? CreadoPorUsuarioId { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public int? ActualizadoPorUsuarioId { get; set; }
    public DateTime? EliminadoEn { get; set; }
    public int? EliminadoPorUsuarioId { get; set; }
    public bool IsDeleted { get; set; }
    public byte[]? RowVersion { get; set; } = null!;
}
