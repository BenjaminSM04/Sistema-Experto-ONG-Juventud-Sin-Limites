using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class ReglaParametro : BaseEntity
{
    public int ReglaParametroId { get; set; }
    public int ReglaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoParametro Tipo { get; set; }
    public string Valor { get; set; } = string.Empty;

  // Navegación
    public Regla Regla { get; set; } = null!;
}
