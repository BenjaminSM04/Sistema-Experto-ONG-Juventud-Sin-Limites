using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POACampoOpcion : BaseEntity
{
    public int OpcionId { get; set; }
    public int CampoId { get; set; }
    public string Valor { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public int Orden { get; set; } = 1;

    // Navegación
    public POACampo Campo { get; set; } = null!;
}
