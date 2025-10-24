using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class DiccionarioObservaciones : BaseEntity
{
  public int DiccionarioId { get; set; }
    public string Expresion { get; set; } = string.Empty;
    public decimal Ponderacion { get; set; }
    public AmbitoDiccionario Ambito { get; set; } = AmbitoDiccionario.Global;
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<DiccionarioObservacionesPrograma> DiccionarioProgramas { get; set; } = new List<DiccionarioObservacionesPrograma>();
}
