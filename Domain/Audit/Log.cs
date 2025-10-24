using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Audit;

public class Log
{
    public long LogId { get; set; }
    public DateTime FechaEventoUtc { get; set; } = DateTime.UtcNow;
    public int? UsuarioActorId { get; set; }
    public string Operacion { get; set; } = string.Empty; // Insert/Update/SoftDelete/Restore
    public string Tabla { get; set; } = string.Empty;
    public string ClavePrimaria { get; set; } = string.Empty;
    public string? DatosAnteriores { get; set; }
    public string? DatosNuevos { get; set; }
    public Guid? TransaccionId { get; set; }
    public string? Origen { get; set; }
    public string? Comentario { get; set; }

    // Navegación
    public Security.Usuario? UsuarioActor { get; set; }
}
