using Microsoft.AspNetCore.Identity;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

/// <summary>
/// Rol del sistema que integra ASP.NET Core Identity
/// </summary>
public class Rol : IdentityRole<int>, IAuditable, ISoftDelete
{
    // Descripción adicional del rol
    public string? Descripcion { get; set; }

    // Campos de auditoría (IAuditable)
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public int? CreadoPorUsuarioId { get; set; }
    public DateTime? ActualizadoEn { get; set; }
    public int? ActualizadoPorUsuarioId { get; set; }

    // Campos de Soft Delete (ISoftDelete)
    public DateTime? EliminadoEn { get; set; }
    public int? EliminadoPorUsuarioId { get; set; }
    public bool IsDeleted { get; set; }

    // RowVersion para concurrencia
    public byte[] RowVersion { get; set; } = null!;

    // Navegación
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();

    // Nota: Ya no necesitamos RolId ni Nombre porque IdentityRole<int> ya los proporciona:
    // - Id (de IdentityRole<int>)
    // - Name (de IdentityRole<int>)
    // - NormalizedName (de IdentityRole<int>)
    // - ConcurrencyStamp (de IdentityRole<int>)
}
