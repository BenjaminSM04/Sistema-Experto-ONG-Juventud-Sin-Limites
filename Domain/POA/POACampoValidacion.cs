using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POACampoValidacion : BaseEntity
{
    public int ValidacionId { get; set; }
    public int CampoId { get; set; }
  public TipoValidacion Tipo { get; set; }
    public string Parametro { get; set; } = string.Empty;

    // Navegación
    public POACampo Campo { get; set; } = null!;
}
