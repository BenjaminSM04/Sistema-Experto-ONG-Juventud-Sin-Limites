using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Programas;

public class Programa : BaseEntity
{
    public int ProgramaId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public EstadoGeneral Estado { get; set; } = EstadoGeneral.Activo;
    public bool InferenciaActiva { get; set; } = true;

    // Navegación
    public ICollection<UsuarioPrograma> UsuarioProgramas { get; set; } = new List<UsuarioPrograma>();
    public ICollection<Operacion.Actividad> Actividades { get; set; } = new List<Operacion.Actividad>();
    public ICollection<POA.POAPlantilla> Plantillas { get; set; } = new List<POA.POAPlantilla>();
    public ICollection<POA.POAInstancia> Instancias { get; set; } = new List<POA.POAInstancia>();
}
