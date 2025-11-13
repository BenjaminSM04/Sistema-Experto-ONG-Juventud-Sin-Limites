using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Api.Models;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Components;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Account;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Extensions;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Interceptors;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Middleware;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Security;
using System.Security.Claims;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
          .AddInteractiveServerComponents(options => options.DetailedErrors = true);

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            // Servicios del Motor de Inferencia
            builder.Services.AddScoped<IFeatureProvider, FeatureProvider>();
            builder.Services.AddScoped<IMotorInferencia, MotorInferencia>();
            builder.Services.AddHostedService<MotorScheduler>();

            // Servicio de Permisos
            builder.Services.AddScoped<IPermissionProvider, PermissionProvider>();

            // Servicio de Reportes
            builder.Services.AddScoped<Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.ReportesProgramaService>();

         // MudBlazor
            builder.Services.AddMudServices();

            // Configurar HttpClient para llamadas a la API desde Blazor
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            // Agregar servicios para API
            builder.Services.AddControllers();
            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
     .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Registrar el interceptor de auditoría
            builder.Services.AddSingleton<AuditableSaveChangesInterceptor>();

            // Configurar DbContext con el interceptor
            builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<AuditableSaveChangesInterceptor>();
                options.UseSqlServer(connectionString)
            .AddInterceptors(interceptor);
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configurar Identity con Usuario y Rol personalizados
            builder.Services.AddIdentityCore<Usuario>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;

    // Políticas de contraseña más restrictivas
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12; // Aumentado a 12 caracteres mínimo
    options.Password.RequiredUniqueChars = 4;

    options.User.RequireUniqueEmail = true;

    // Configuración de Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
           .AddRoles<Rol>() // Agregar soporte para roles personalizados
    .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddSignInManager()
        .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<Usuario>, IdentityNoOpEmailSender>();

            var app = builder.Build();

            // Inicializar base de datos (Opcional - comentar si no quieres seed automático)
            if (app.Environment.IsDevelopment())
            {
                try
                {
                    await app.Services.InitializeDatabaseAsync();
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Error al inicializar la base de datos");
                    // Continuar la ejecución incluso si falla el seeder
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            // ⚠️ IMPORTANTE: Middleware de cambio obligatorio de contraseña
            // Debe estar después de UseAuthentication y UseAuthorization
            app.UseForceChangePassword();

            // ========================================
            // MINIMAL API ENDPOINTS
            // ========================================

            var apiGroup = app.MapGroup("/api")
                 .RequireAuthorization();

            // ========================================
            // 1) POST /api/motor/ejecutar
            // ========================================
            apiGroup.MapPost("/motor/ejecutar", async (
        [FromBody] MotorRunDto request,
           IMotorInferencia motor,
             ApplicationDbContext context,
               ClaimsPrincipal user,
           ILogger<Program> logger,
                 CancellationToken ct) =>
             {
                 app.Logger.LogInformation("🔍 Endpoint /api/motor/ejecutar llamado");
                 app.Logger.LogInformation($"Usuario: {user.Identity?.Name}, Autenticado: {user.Identity?.IsAuthenticated}");
                 app.Logger.LogInformation($"Roles: {string.Join(", ", user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

                 // Verificar roles permitidos
                 if (!user.IsInRole("Administrador") && !user.IsInRole("Coordinador"))
                 {
                     app.Logger.LogWarning("❌ Usuario sin permisos suficientes");
                     return Results.Forbid();
                 }

                 // Fecha de corte (default: hoy)
                 var fechaCorte = request.FechaCorte ?? DateTime.UtcNow;
                 var fechaCorteOnly = DateOnly.FromDateTime(fechaCorte);

                 app.Logger.LogInformation($"📅 Ejecutando motor: FechaCorte={fechaCorteOnly}, ProgramaId={request.ProgramaId}");

                 // Ejecutar motor
                 try
                 {
                     var resumen = await motor.EjecutarAsync(fechaCorteOnly, request.ProgramaId, ct);

                     app.Logger.LogInformation($"✅ Motor ejecutado: Reglas={resumen.ReglasEjecutadas}, Alertas={resumen.AlertasGeneradas}, Errores={resumen.Errores}");

                     // Obtener últimas 50 alertas generadas
                     var ultimasAlertas = await context.Alertas
        .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.AlertaId)
        .Take(50)
        .Select(a => new AlertaDto(
         a.AlertaId,
            a.Mensaje,
        (byte)a.Severidad,
           (byte)a.Estado,
       a.GeneradaEn,
            a.ReglaId,
               a.ProgramaId,
         a.ActividadId,
           a.ParticipanteId,
        a.RowVersion
        ))
               .ToListAsync(ct);

                     app.Logger.LogInformation($"📋 Obtenidas {ultimasAlertas.Count} alertas de la BD");

                     var response = new MotorRunResponseDto(
            fechaCorte,
                    request.ProgramaId,
                         new ResumenEjecucion(
              resumen.ReglasEjecutadas,
                resumen.AlertasGeneradas,
              resumen.Errores
            ),
                 ultimasAlertas
                  );

                     return Results.Ok(response);
                 }
                 catch (Exception ex)
                 {
                     app.Logger.LogError(ex, "❌ Error ejecutando motor");
                     return Results.Problem(detail: ex.Message, statusCode: 500);
                 }
             })
         .WithName("EjecutarMotor")
          .Produces<MotorRunResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

            // ========================================
            // 2) GET /api/alertas
            // ========================================
            apiGroup.MapGet("/alertas", async (
             ApplicationDbContext context,
         [FromQuery] int? programaId,
                  [FromQuery] DateTime? desde,
      [FromQuery] DateTime? hasta,
        [FromQuery] byte? severidad,
      [FromQuery] byte? estado,
       ClaimsPrincipal user,
         CancellationToken ct) =>
     {
         // Verificar roles permitidos
         if (!user.IsInRole("Administrador") &&
           !user.IsInRole("Coordinador") &&
   !user.IsInRole("Facilitador") &&
       !user.IsInRole("Visualizador"))
         {
             return Results.Forbid();
         }

         var query = context.Alertas
         .Where(a => !a.IsDeleted);

         // Aplicar filtros opcionales
         if (programaId.HasValue)
             query = query.Where(a => a.ProgramaId == programaId.Value);

         if (desde.HasValue)
             query = query.Where(a => a.GeneradaEn >= desde.Value);

         if (hasta.HasValue)
             query = query.Where(a => a.GeneradaEn <= hasta.Value);

         if (severidad.HasValue)
             query = query.Where(a => (byte)a.Severidad == severidad.Value);

         if (estado.HasValue)
             query = query.Where(a => (byte)a.Estado == estado.Value);

         // Obtener máximo 200 alertas
         var alertas = await query
             .OrderByDescending(a => a.AlertaId)
                  .Take(200)
    .Select(a => new AlertaDto(
             a.AlertaId,
         a.Mensaje,
                   (byte)a.Severidad,
        (byte)a.Estado,
            a.GeneradaEn,
      a.ReglaId,
         a.ProgramaId,
               a.ActividadId,
        a.ParticipanteId,
               a.RowVersion
            ))
        .ToListAsync(ct);

         return Results.Ok(alertas);
     })
     .WithName("ObtenerAlertas")
         .Produces<List<AlertaDto>>(StatusCodes.Status200OK)
       .Produces(StatusCodes.Status403Forbidden);

            // ========================================
            // 3) PATCH /api/alertas/{id}/estado
            // ========================================
            apiGroup.MapPatch("/alertas/{id:int}/estado", async (
                         int id,
               [FromBody] AlertaCambioEstadoDto request,
                      ApplicationDbContext context,
                   ClaimsPrincipal user,
                    CancellationToken ct) =>
              {
                  // Verificar roles permitidos
                  if (!user.IsInRole("Administrador") && !user.IsInRole("Coordinador"))
                  {
                      return Results.Forbid();
                  }

                  // Buscar alerta
                  var alerta = await context.Alertas
 .Where(a => a.AlertaId == id && !a.IsDeleted)
.FirstOrDefaultAsync(ct);

                  if (alerta == null)
                  {
                      return Results.NotFound(new { error = "Alerta no encontrada" });
                  }

                  // Manejo de concurrencia optimista
                  if (request.RowVersion != null && request.RowVersion.Length > 0)
                  {
                      context.Entry(alerta).Property(a => a.RowVersion).OriginalValue = request.RowVersion;
                  }

                  // Obtener ID del usuario actuales
                  var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                  int? userId = null;
                  if (int.TryParse(userIdClaim, out int parsedUserId))
                  {
                      userId = parsedUserId;
                  }

                  // Actualizar alerta
                  alerta.Estado = (Domain.Common.EstadoAlerta)request.NuevoEstado;
                  if (!string.IsNullOrWhiteSpace(request.Comentario))
                  {
                      alerta.Notas = request.Comentario;
                  }
                  alerta.ActualizadoEn = DateTime.UtcNow;
                  alerta.ActualizadoPorUsuarioId = userId;

                  // Si se marca como resuelta, guardar fecha y usuario
                  if (request.NuevoEstado == (byte)Domain.Common.EstadoAlerta.Resuelta)
                  {
                      alerta.ResueltaEn = DateTime.UtcNow;
                      alerta.ResueltaPorUsuarioId = userId;
                  }

                  try
                  {
                      await context.SaveChangesAsync(ct);
                      return Results.NoContent();
                  }
                  catch (DbUpdateConcurrencyException)
                  {
                      return Results.Conflict(new
                      {
                          error = "La alerta fue modificada por otro usuario. Por favor, recarga los datos."
                      });
                  }
              })
                       .WithName("CambiarEstadoAlerta")
                    .Produces(StatusCodes.Status204NoContent)
           .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status409Conflict)
                       .Produces(StatusCodes.Status403Forbidden);

            // ========================================
            // FIN DE MINIMAL API ENDPOINTS
            // ========================================

            // ========================================
            // ENDPOINT DE DIAGNÓSTICO (Solo Development)
            // ========================================
            if (app.Environment.IsDevelopment())
            {
                apiGroup.MapGet("/motor/diagnostico", async (
                  ApplicationDbContext context,
                    ClaimsPrincipal user) =>
                   {
                       if (!user.IsInRole("Administrador") && !user.IsInRole("Coordinador"))
                           return Results.Forbid();

                       var diagnostico = new
                       {
                           Reglas = await context.Reglas.Where(r => !r.IsDeleted).CountAsync(),
                           ReglasActivas = await context.Reglas.Where(r => r.Activa && !r.IsDeleted).CountAsync(),
                           Parametros = await context.ReglaParametros.Where(p => !p.IsDeleted).CountAsync(),
                           Programas = await context.Programas.Where(p => !p.IsDeleted).CountAsync(),
                           Participantes = await context.Participantes.Where(p => !p.IsDeleted).CountAsync(),
                           Actividades = await context.Actividades.Where(a => !a.IsDeleted).CountAsync(),
                           Asistencias = await context.Asistencias.Where(a => !a.IsDeleted).CountAsync(),
                           Metricas = await context.MetricasProgramaMes.Where(m => !m.IsDeleted).CountAsync(),
                           Alertas = await context.Alertas.Where(a => !a.IsDeleted).CountAsync(),
                           EjecucionesMotor = await context.EjecucionesMotor.CountAsync(),
                           UltimaEjecucion = await context.EjecucionesMotor
                .OrderByDescending(e => e.EjecucionId)
            .Select(e => new { e.InicioUtc, e.ResultadoResumen })
           .FirstOrDefaultAsync(),
                           ReglasPorObjetivo = await context.Reglas
             .Where(r => !r.IsDeleted && r.Activa)
       .GroupBy(r => r.Objetivo)
                 .Select(g => new { Objetivo = g.Key.ToString(), Count = g.Count() })
           .ToListAsync()
                       };

                       return Results.Ok(diagnostico);
                   })
                     .WithName("DiagnosticoMotor")
                .Produces<object>(StatusCodes.Status200OK);
            }

            app.MapControllers();

            app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();


            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
