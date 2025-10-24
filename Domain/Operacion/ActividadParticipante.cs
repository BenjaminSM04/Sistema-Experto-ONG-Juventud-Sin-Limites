using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

public class ActividadParticipante : BaseEntity
{
    public int ActividadId { get; set; }
    public int ParticipanteId { get; set; }
    public RolParticipante Rol { get; set; } = RolParticipante.Asistente;
    public EstadoInscripcion Estado { get; set; } = EstadoInscripcion.Inscrito;

    // Navegación
    public Actividad Actividad { get; set; } = null!;
    public Participante Participante { get; set; } = null!;
}
