using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.POA;

public class POAArchivo : BaseEntity
{
    public int ArchivoId { get; set; }
    public int InstanciaId { get; set; }
    public int? CampoId { get; set; }
    public string ArchivoPath { get; set; } = string.Empty;
    public int? SubidoPorUsuarioId { get; set; }
    public DateTime SubidoEn { get; set; } = DateTime.UtcNow;

 // Navegación
    public POAInstancia Instancia { get; set; } = null!;
    public POACampo? Campo { get; set; }
}
