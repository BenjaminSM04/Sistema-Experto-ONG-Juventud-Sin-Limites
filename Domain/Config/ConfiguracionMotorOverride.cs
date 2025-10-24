using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Config;

public class ConfiguracionMotorOverride : BaseEntity
{
    public int ProgramaId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Version { get; set; } = 1;

    // Navegación
    public Programas.Programa Programa { get; set; } = null!;
    public ConfiguracionMotor ConfiguracionBase { get; set; } = null!;
}
