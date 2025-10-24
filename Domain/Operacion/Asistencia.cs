using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

public class Asistencia : BaseEntity
{
    public int AsistenciaId { get; set; }
    public int ActividadId { get; set; }
    public int ParticipanteId { get; set; }
    public DateTime Fecha { get; set; }
    public EstadoAsistencia Estado { get; set; }
    public string? Observacion { get; set; }

    // Navegación
    public Actividad Actividad { get; set; } = null!;
    public Participante Participante { get; set; } = null!;
}
