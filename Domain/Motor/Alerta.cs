using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class Alerta : BaseEntity
{
    public int AlertaId { get; set; }
    public int ReglaId { get; set; }
    public Severidad Severidad { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public EstadoAlerta Estado { get; set; } = EstadoAlerta.Abierta;

    public int? ProgramaId { get; set; }
    public int? ActividadId { get; set; }
    public int? ParticipanteId { get; set; }

    public DateTime GeneradaEn { get; set; } = DateTime.UtcNow;
    public int? ResueltaPorUsuarioId { get; set; }
    public DateTime? ResueltaEn { get; set; }
    public string? Notas { get; set; }

    // Navegación
    public Regla Regla { get; set; } = null!;
    public Programas.Programa? Programa { get; set; }
    public Operacion.Actividad? Actividad { get; set; }
    public Operacion.Participante? Participante { get; set; }
}
