using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class RiesgoDetalle : BaseEntity
{
    public int RiesgoDetalleId { get; set; }
  public int RiesgoId { get; set; }
    public string NombreFeature { get; set; } = string.Empty;
    public decimal? ValorNumerico { get; set; }
    public string? ValorTexto { get; set; }
    public decimal PesoContribucion { get; set; }

    // Navegación
    public RiesgoParticipantePrograma Riesgo { get; set; } = null!;
}
