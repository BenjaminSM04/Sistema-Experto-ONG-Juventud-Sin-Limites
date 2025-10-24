using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POAPlantilla : BaseEntity
{
    public int PlantillaId { get; set; }
    public int ProgramaId { get; set; }
    public int Version { get; set; }
    public EstadoPlantilla Estado { get; set; } = EstadoPlantilla.Borrador;
    public DateTime? VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }

    // Navegación
    public Programas.Programa Programa { get; set; } = null!;
    public ICollection<POAPlantillaSeccion> Secciones { get; set; } = new List<POAPlantillaSeccion>();
    public ICollection<POACampo> Campos { get; set; } = new List<POACampo>();
    public ICollection<POAInstancia> Instancias { get; set; } = new List<POAInstancia>();
}
