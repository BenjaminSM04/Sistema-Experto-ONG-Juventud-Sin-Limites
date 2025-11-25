using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Middleware;

/// <summary>
/// Middleware global para manejo centralizado de excepciones
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log del error
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        _logger.LogError(exception, 
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}, User: {User}",
            traceId,
            context.Request.Path,
            context.Request.Method,
            context.User?.Identity?.Name ?? "Anonymous");

        // Determinar código de estado
        var statusCode = exception switch
        {
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        // Para solicitudes de API (JSON)
        if (context.Request.Path.StartsWithSegments("/api") || 
            context.Request.Headers.Accept.ToString().Contains("application/json"))
        {
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An error occurred while processing your request.",
                TraceId = traceId,
                Detail = _environment.IsDevelopment() 
                    ? exception.StackTrace 
                    : null
            };

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await context.Response.WriteAsync(json);
        }
        else
        {
            // Para solicitudes de páginas (Blazor), redirigir a página de error
            if (!context.Response.HasStarted)
            {
                // Guardar información del error en Items para acceso desde la página de error
                context.Items["OriginalException"] = exception;
                context.Items["OriginalPath"] = context.Request.Path.Value;
                
                // Redirigir según el código de error
                var errorPath = statusCode == HttpStatusCode.NotFound 
                    ? "/NotFound" 
                    : "/Error";

                context.Response.Redirect(errorPath);
            }
        }
    }
}

/// <summary>
/// Extensiones para registrar el middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// Registra el middleware de manejo global de excepciones
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
