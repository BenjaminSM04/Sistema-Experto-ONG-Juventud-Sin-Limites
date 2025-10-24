using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class EjecucionMotor : BaseEntity
{
 public int EjecucionId { get; set; }
    public DateTime InicioUtc { get; set; }
    public DateTime? FinUtc { get; set; }
    public AmbitoEjecucion Ambito { get; set; }
    public string? ResultadoResumen { get; set; }
    public int Exitos { get; set; }
    public int Errores { get; set; }
 public Guid? TransaccionId { get; set; }

    // Navegación
    public ICollection<MatchRegla> Matches { get; set; } = new List<MatchRegla>();
}
