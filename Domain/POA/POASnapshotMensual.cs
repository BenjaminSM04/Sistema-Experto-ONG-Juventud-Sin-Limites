using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POASnapshotMensual : BaseEntity
{
    public int ProgramaId { get; set; }
    public string AnioMes { get; set; } = string.Empty; // 'YYYY-MM'
public int PlantillaVersion { get; set; }
    public string PayloadJson { get; set; } = string.Empty;

    // Navegación
    public Programas.Programa Programa { get; set; } = null!;
}
