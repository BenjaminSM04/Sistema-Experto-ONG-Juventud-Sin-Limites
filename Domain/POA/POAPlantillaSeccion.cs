using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POAPlantillaSeccion : BaseEntity
{
    public int SeccionId { get; set; }
    public int PlantillaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; } = 1;

    // Navegación
    public POAPlantilla Plantilla { get; set; } = null!;
    public ICollection<POACampo> Campos { get; set; } = new List<POACampo>();
}
