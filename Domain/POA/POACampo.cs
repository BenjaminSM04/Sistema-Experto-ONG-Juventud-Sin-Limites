using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POACampo : BaseEntity
{
 public int CampoId { get; set; }
    public int PlantillaId { get; set; }
    public string Clave { get; set; } = string.Empty;
  public string Etiqueta { get; set; } = string.Empty;
  public TipoDato TipoDato { get; set; }
    public bool Requerido { get; set; }
    public int Orden { get; set; } = 1;
    public string? Unidad { get; set; }
    public AlcancePOA Alcance { get; set; } = AlcancePOA.Programa;
    public int? SeccionId { get; set; }

    // Navegación
    public POAPlantilla Plantilla { get; set; } = null!;
    public POAPlantillaSeccion? Seccion { get; set; }
    public ICollection<POACampoOpcion> Opciones { get; set; } = new List<POACampoOpcion>();
    public ICollection<POACampoValidacion> Validaciones { get; set; } = new List<POACampoValidacion>();
    public ICollection<POAValor> Valores { get; set; } = new List<POAValor>();
}
