using Microsoft.AspNetCore.Identity;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using System.Security.Claims;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Middleware;

/// <summary>
/// Middleware que fuerza al usuario a cambiar su contraseña si MustChangePassword == true
/// </summary>
public class ForceChangePasswordMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ForceChangePasswordMiddleware> _logger;

    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Account/ForceChangePassword",
        "/Account/Manage",  
        "/Account/Logout",
        "/_framework",
        "/_blazor",
        "/css",
        "/js",
        "/favicon.ico",
        "/api"
    };

    public ForceChangePasswordMiddleware(RequestDelegate next, ILogger<ForceChangePasswordMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<Usuario> userManager)
    {
        // Solo aplicar a usuarios autenticados
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Permitir rutas específicas
        var path = context.Request.Path.Value ?? "";
        if (AllowedPaths.Any(allowed => path.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Obtener usuario actual
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            await _next(context);
            return;
        }

        // Verificar si debe cambiar contraseña
        if (user.MustChangePassword)
        {
            _logger.LogInformation("Usuario {Email} debe cambiar su contraseña, redirigiendo...", user.Email);
            context.Response.Redirect("/Account/ForceChangePassword");
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method para registrar el middleware
/// </summary>
public static class ForceChangePasswordMiddlewareExtensions
{
    public static IApplicationBuilder UseForceChangePassword(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ForceChangePasswordMiddleware>();
    }
}
