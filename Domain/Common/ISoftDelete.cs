namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

public interface ISoftDelete
{
    DateTime? EliminadoEn { get; set; }
    int? EliminadoPorUsuarioId { get; set; }
    bool IsDeleted { get; set; }
}
