using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Operacion;

public class EvidenciaActividad : BaseEntity
{
    public int EvidenciaId { get; set; }
    public int ActividadId { get; set; }
    public TipoEvidencia Tipo { get; set; }
    public string ArchivoPath { get; set; } = string.Empty;
    public int? SubidoPorUsuarioId { get; set; }
    public DateTime SubidoEn { get; set; } = DateTime.UtcNow;

    // Navegación
  public Actividad Actividad { get; set; } = null!;
}
