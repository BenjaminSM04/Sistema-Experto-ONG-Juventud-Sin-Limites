using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class DiccionarioObservacionesPrograma : BaseEntity
{
    public int DiccionarioId { get; set; }
    public int ProgramaId { get; set; }

    // Navegación
    public DiccionarioObservaciones Diccionario { get; set; } = null!;
    public Programas.Programa Programa { get; set; } = null!;
}
