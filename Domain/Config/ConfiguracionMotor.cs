using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Config;

public class ConfiguracionMotor : BaseEntity
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Version { get; set; } = 1;

    // Navegación
    public ICollection<ConfiguracionMotorOverride> Overrides { get; set; } = new List<ConfiguracionMotorOverride>();
}
