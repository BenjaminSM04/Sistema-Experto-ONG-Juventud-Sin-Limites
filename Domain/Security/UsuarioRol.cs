using Microsoft.AspNetCore.Identity;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

/// <summary>
/// Relación Usuario-Rol que extiende IdentityUserRole
/// </summary>
public class UsuarioRol : IdentityUserRole<int>, IAuditable, ISoftDelete
{
    public DateTime AsignadoEn { get; set; } = DateTime.UtcNow;

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
    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;

    // Nota: IdentityUserRole<int> ya proporciona:
    // - UserId (lo usamos como UsuarioId)
    // - RoleId (lo usamos como RolId)
}
