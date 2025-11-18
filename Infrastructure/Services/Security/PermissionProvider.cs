using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using System.Security.Claims;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Security;

/// <summary>
/// Implementación del proveedor de permisos
/// </summary>
public class PermissionProvider : IPermissionProvider
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionProvider(
        AuthenticationStateProvider authStateProvider,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _authStateProvider = authStateProvider;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> CanAccessAsync(Feature feature, int? programaId = null, int? actividadId = null)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        // Administrador tiene acceso a todo
        if (IsInRole(user, "Administrador"))
            return true;

        // Mapeo de features por rol
        return feature switch
        {
            // ========== VISUALIZACIÓN ==========
            Feature.Ver_Dashboard_Completo => IsInRole(user, "Administrador"),
            Feature.Ver_Dashboard_Programa => await CanAccessProgramaAsync(user, programaId),
            Feature.Ver_Dashboard_Actividad => await CanAccessActividadAsync(user, actividadId),
            Feature.Ver_Presupuesto => IsInRole(user, "Administrador"),
            Feature.Ver_Finanzas => IsInRole(user, "Administrador"),
            Feature.Ver_DatosSensibles => IsInRole(user, "Administrador"),
            Feature.Ver_Asistencia => IsInRole(user, "Administrador", "Coordinador", "Facilitador"),
            Feature.Ver_Participantes => IsInRole(user, "Administrador", "Coordinador", "Facilitador"),
            Feature.Ver_Alertas => IsInRole(user, "Administrador", "Coordinador"),
            Feature.Ver_Metricas => IsInRole(user, "Administrador", "Coordinador", "Visualizador"),
            Feature.Ver_POA => IsInRole(user, "Administrador", "Coordinador"),

            // ========== GESTIÓN ==========
            Feature.Gestionar_Usuarios => IsInRole(user, "Administrador"),
            Feature.Gestionar_Roles => IsInRole(user, "Administrador"),
            Feature.Gestionar_Programas => IsInRole(user, "Administrador"),
            Feature.Gestionar_Actividades => IsInRole(user, "Administrador", "Coordinador"),
            Feature.Gestionar_Participantes => IsInRole(user, "Administrador", "Coordinador"),
            Feature.Gestionar_POA => IsInRole(user, "Administrador", "Coordinador"),

            // ========== MOTOR ==========
            Feature.Ejecutar_Motor => IsInRole(user, "Administrador", "Coordinador"),
            Feature.Configurar_Motor => IsInRole(user, "Administrador"),
            Feature.Ver_Logs_Motor => IsInRole(user, "Administrador", "Coordinador"),
            Feature.Gestionar_Reglas => IsInRole(user, "Administrador"),

            // ========== REPORTES ==========
            Feature.Generar_Reportes_Completos => IsInRole(user, "Administrador"),
            Feature.Generar_Reportes_Programa => IsInRole(user, "Administrador", "Coordinador"),
            Feature.Exportar_Datos => IsInRole(user, "Administrador", "Coordinador"),

            // ========== ASISTENCIA ==========
            Feature.Registrar_Asistencia => IsInRole(user, "Administrador", "Coordinador", "Facilitador"),
            Feature.Modificar_Asistencia => IsInRole(user, "Administrador", "Coordinador"),

            // ========== EVIDENCIAS ==========
            Feature.Subir_Evidencias => IsInRole(user, "Administrador", "Coordinador", "Facilitador"),
            Feature.Eliminar_Evidencias => IsInRole(user, "Administrador", "Coordinador"),

            // ========== METABASE ==========
            Feature.Acceso_Metabase_Admin => IsInRole(user, "Administrador"),
            Feature.Acceso_Metabase_Coordinador => IsInRole(user, "Administrador", "Coordinador"),
            Feature.Acceso_Metabase_Lectura => IsInRole(user, "Administrador", "Coordinador", "Visualizador"),

            // ========== AUDITORÍA ==========
            Feature.Ver_Logs_Auditoria => IsInRole(user, "Administrador"),
            Feature.Ver_Historial_Cambios => IsInRole(user, "Administrador", "Coordinador"),

            _ => false
        };
    }

    public async Task<List<Feature>> GetUserFeaturesAsync()
    {
        var features = new List<Feature>();
        var allFeatures = Enum.GetValues<Feature>();

        foreach (var feature in allFeatures)
        {
            if (await CanAccessAsync(feature))
            {
                features.Add(feature);
            }
        }

        return features;
    }

    public async Task<List<int>> GetUserProgramsAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return new List<int>();

        // Administrador ve todos los programas
        if (IsInRole(user, "Administrador"))
        {
            return await _context.Programas
                 .Where(p => !p.IsDeleted)
                     .Select(p => p.ProgramaId)
                 .ToListAsync();
        }

        // Coordinador solo ve programas asignados
        var userId = GetUserId(user);
        if (userId.HasValue)
        {
            return await _context.UsuarioProgramas
  .Where(up => up.UsuarioId == userId.Value && !up.IsDeleted)
    .Select(up => up.ProgramaId)
       .ToListAsync();
        }

        return new List<int>();
    }

    public bool IsAdministrador()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.IsInRole("Administrador") ?? false;
    }

    public async Task<bool> IsCoordinadorAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        if (IsInRole(user, "Coordinador"))
        {
            var userId = GetUserId(user);
            if (userId.HasValue)
            {
                // Verificar que tenga al menos un programa asignado
                return await _context.UsuarioProgramas
              .AnyAsync(up => up.UsuarioId == userId.Value && !up.IsDeleted);
            }
        }

        return false;
    }

    public async Task<bool> IsFacilitadorAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return false;

        if (IsInRole(user, "Facilitador"))
        {
            var userId = GetUserId(user);
            if (userId.HasValue)
            {
                // Verificar que tenga al menos una actividad asignada
                // Nota: Esto requiere agregar FacilitadorId a la tabla Actividad
                // Por ahora retornamos true si tiene el rol
                return true;
            }
        }

        return false;
    }

    // ========== MÉTODOS PRIVADOS ==========

    private async Task<ClaimsPrincipal?> GetCurrentUserAsync()
    {
        // Intentar obtener del HttpContext primero (API calls)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User;
        }

        // Si no, obtener del AuthenticationStateProvider (Blazor)
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated == true ? authState.User : null;
    }

    private bool IsInRole(ClaimsPrincipal user, params string[] roles)
    {
        return roles.Any(role => user.IsInRole(role));
    }

    private int? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        return null;
    }

    private async Task<bool> CanAccessProgramaAsync(ClaimsPrincipal user, int? programaId)
    {
        if (!programaId.HasValue) return false;

        // Administrador puede acceder a todos los programas
        if (IsInRole(user, "Administrador"))
            return true;

        // Coordinador solo puede acceder a programas asignados
        if (IsInRole(user, "Coordinador"))
        {
            var userId = GetUserId(user);
            if (userId.HasValue)
            {
                return await _context.UsuarioProgramas
                .AnyAsync(up => up.UsuarioId == userId.Value
                && up.ProgramaId == programaId.Value
                  && !up.IsDeleted);
            }
        }

        return false;
    }

    private async Task<bool> CanAccessActividadAsync(ClaimsPrincipal user, int? actividadId)
    {
        if (!actividadId.HasValue) return false;

        // Administrador puede acceder a todas las actividades
        if (IsInRole(user, "Administrador"))
            return true;

        // Obtener la actividad y su programa
        var actividad = await _context.Actividades
            .Where(a => a.ActividadId == actividadId.Value && !a.IsDeleted)
        .Select(a => new { a.ProgramaId })
         .FirstOrDefaultAsync();

        if (actividad == null) return false;

        // Coordinador puede acceder si el programa está asignado
        if (IsInRole(user, "Coordinador"))
        {
            return await CanAccessProgramaAsync(user, actividad.ProgramaId);
        }

        // Facilitador puede acceder si es facilitador de la actividad
        // Nota: Esto requiere agregar FacilitadorId a la tabla Actividad
        // Por ahora permitimos acceso si tiene el rol y el programa está asignado
        if (IsInRole(user, "Facilitador"))
        {
            return await CanAccessProgramaAsync(user, actividad.ProgramaId);
        }

        return false;
    }
}
