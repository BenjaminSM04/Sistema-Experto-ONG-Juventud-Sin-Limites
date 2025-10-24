using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

public class Participante : BaseEntity
{
    public int ParticipanteId { get; set; }
    public int PersonaId { get; set; }
    public EstadoGeneral Estado { get; set; } = EstadoGeneral.Activo;
    public DateTime FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public string? Observaciones { get; set; }

 // Navegación
    public Security.Persona Persona { get; set; } = null!;
    public ICollection<ActividadParticipante> ActividadParticipantes { get; set; } = new List<ActividadParticipante>();
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
}
