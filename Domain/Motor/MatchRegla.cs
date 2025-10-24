using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class MatchRegla : BaseEntity
{
    public int MatchId { get; set; }
    public int EjecucionId { get; set; }
    public int ReglaId { get; set; }
    public int? ProgramaId { get; set; }
    public int? ActividadId { get; set; }
  public int? ParticipanteId { get; set; }
    public string? Mensaje { get; set; }
    public bool GeneroAlerta { get; set; }

    // Navegación
    public EjecucionMotor Ejecucion { get; set; } = null!;
    public Regla Regla { get; set; } = null!;
    public Programas.Programa? Programa { get; set; }
    public Operacion.Actividad? Actividad { get; set; }
    public Operacion.Participante? Participante { get; set; }
}
