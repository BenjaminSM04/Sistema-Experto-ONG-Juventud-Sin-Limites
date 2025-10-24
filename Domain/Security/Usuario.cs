using Microsoft.AspNetCore.Identity;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Common;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

/// <summary>
/// Usuario del sistema que integra ASP.NET Core Identity con el dominio
/// </summary>
public class Usuario : IdentityUser<int>, IAuditable, ISoftDelete
{
    // Relación 1:1 con Persona (datos personales separados)
  public int PersonaId { get; set; }
    
    // Estado personalizado (además del LockoutEnabled de Identity)
    public EstadoGeneral Estado { get; set; } = EstadoGeneral.Activo;

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
    public Persona Persona { get; set; } = null!;
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public ICollection<Programas.UsuarioPrograma> UsuarioProgramas { get; set; } = new List<Programas.UsuarioPrograma>();

    // Nota: Ya no necesitamos Email, PasswordHash, IntentosFallidos, LockoutHasta
    // porque IdentityUser<int> ya los proporciona:
    // - Email (de IdentityUser)
    // - PasswordHash (de IdentityUser)
    // - AccessFailedCount (equivalente a IntentosFallidos)
    // - LockoutEnd (equivalente a LockoutHasta)
    // - UserName (de IdentityUser)
    // - EmailConfirmed, PhoneNumber, TwoFactorEnabled, etc.
}
