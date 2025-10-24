using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Motor;

public class Regla : BaseEntity
{
    public int ReglaId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Severidad Severidad { get; set; }
    public ObjetivoRegla Objetivo { get; set; }
    public bool Activa { get; set; } = true;
    public int Prioridad { get; set; } = 100;
    public int Version { get; set; } = 1;

    // Navegación
    public ICollection<ReglaParametro> Parametros { get; set; } = new List<ReglaParametro>();
    public ICollection<ReglaParametroOverride> ParametroOverrides { get; set; } = new List<ReglaParametroOverride>();
    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    public ICollection<MatchRegla> Matches { get; set; } = new List<MatchRegla>();
}
