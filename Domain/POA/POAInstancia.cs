using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POAInstancia : BaseEntity
{
    public int InstanciaId { get; set; }
    public int ProgramaId { get; set; }
    public int PlantillaId { get; set; }
    public short PeriodoAnio { get; set; }
    public byte? PeriodoMes { get; set; }
    public EstadoInstancia Estado { get; set; } = EstadoInstancia.Borrador;
    public string? Notas { get; set; }

    // Navegación
    public Programas.Programa Programa { get; set; } = null!;
    public POAPlantilla Plantilla { get; set; } = null!;
    public ICollection<POAValor> Valores { get; set; } = new List<POAValor>();
    public ICollection<POAArchivo> Archivos { get; set; } = new List<POAArchivo>();
}
