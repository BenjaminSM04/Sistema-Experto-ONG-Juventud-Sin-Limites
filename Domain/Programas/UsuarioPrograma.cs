using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;

public class UsuarioPrograma : BaseEntity
{
    public int UsuarioId { get; set; }
    public int ProgramaId { get; set; }
    public DateTime Desde { get; set; }
    public DateTime? Hasta { get; set; }

    // Navegación
    public Security.Usuario Usuario { get; set; } = null!;
    public Programa Programa { get; set; } = null!;
}
