using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

public class Actividad : BaseEntity
{
    public int ActividadId { get; set; }
    public int ProgramaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
  public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Lugar { get; set; }
    public TipoActividad Tipo { get; set; } = TipoActividad.Taller;
    public EstadoActividad Estado { get; set; } = EstadoActividad.Planificada;

  // Navegación
    public Programas.Programa Programa { get; set; } = null!;
    public ICollection<ActividadParticipante> ActividadParticipantes { get; set; } = new List<ActividadParticipante>();
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
    public ICollection<EvidenciaActividad> Evidencias { get; set; } = new List<EvidenciaActividad>();
}
