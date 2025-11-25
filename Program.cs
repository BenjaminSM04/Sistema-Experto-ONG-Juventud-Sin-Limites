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
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Validation;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using AspNetCoreRateLimit;

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

            // Servicios de Validación y Sanitización
            builder.Services.AddSingleton<HtmlSanitizerService>();

            // Servicios de Autenticación y Email
            builder.Services.AddSingleton<EmailTwoFactorService>();
            builder.Services.AddScoped<EmailSenderService>();

            // Servicio de Reportes
            builder.Services.AddScoped<Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.ReportesProgramaService>();
            builder.Services.AddScoped<Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.ReportesMotorService>();

            // Servicio del Motor
            builder.Services.AddScoped<Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.MotorService>();
            builder.Services.AddScoped<Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.ReglaAdminService>();

            // Servicio del Clima
            builder.Services.AddScoped<Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.WeatherService>();
            
            // Servicio de POA
            builder.Services.AddScoped<Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.POAService>();
            
            // Servicio de Exportación POA a PDF
            builder.Services.AddScoped<Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.POAPdfExportService>();

         // MudBlazor
            builder.Services.AddMudServices();

            // Configurar HttpClient para llamadas a la API desde Blazor
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            
            // Configurar HttpClient con BaseAddress para componentes Blazor
            builder.Services.AddScoped(sp =>
            {
                var accessor = sp.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext?.Request;
                var baseUrl = request != null 
                    ? $"{request.Scheme}://{request.Host}" 
                    : "https://localhost:7001"; // Fallback para desarrollo
                return new HttpClient { BaseAddress = new Uri(baseUrl) };
            });

            // Configurar Rate Limiting
            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

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

            // Reemplazar el sender de emails por defecto con nuestro servicio personalizado
            builder.Services.AddScoped<IEmailSender<Usuario>, EmailSenderService>();
            builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSenderService>();

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
                // En producción, usar middleware global de excepciones
                app.UseGlobalExceptionHandler();
                app.UseHsts();
            }

            // Middleware global de manejo de excepciones (también en desarrollo para consistencia)
            if (app.Environment.IsDevelopment())
            {
                app.UseGlobalExceptionHandler();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            // Rate Limiting Middleware
            app.UseIpRateLimiting();

            // 🔒 IMPORTANTE: Middleware de cambio obligatorio de contraseña
            // Debe estar después de UseAuthentication y UseAuthorization
            app.UseForceChangePassword();

            // ========================================
            // MINIMAL API ENDPOINTS
            // ========================================

            var apiGroup = app.MapGroup("/api")
                 .RequireAuthorization();

            // CRUD Reglas (solo Administrador)
            var reglasGroup = apiGroup.MapGroup("/reglas");
            reglasGroup.MapGet("", async (Infrastructure.Services.ReglaAdminService svc, ClaimsPrincipal user, CancellationToken ct) =>
            {
                if (!user.IsInRole("Administrador")) return Results.Forbid();
                var lista = await svc.ListarAsync(ct);
                return Results.Ok(lista);
            });
            reglasGroup.MapGet("/{id:int}", async (int id, Infrastructure.Services.ReglaAdminService svc, ClaimsPrincipal user, CancellationToken ct) =>
            {
                if (!user.IsInRole("Administrador")) return Results.Forbid();
                var regla = await svc.ObtenerAsync(id, ct);
                return regla == null ? Results.NotFound() : Results.Ok(regla);
            });
            reglasGroup.MapPost("", async ([FromBody] UpsertReglaRequest req, Infrastructure.Services.ReglaAdminService svc, ClaimsPrincipal user, CancellationToken ct) =>
            {
                if (!user.IsInRole("Administrador")) return Results.Forbid();
                var dto = await svc.UpsertAsync(req, ct);
                return Results.Created($"/api/reglas/{dto.ReglaId}", dto);
            });
            reglasGroup.MapPut("/{id:int}", async (int id, [FromBody] UpsertReglaRequest req, Infrastructure.Services.ReglaAdminService svc, ClaimsPrincipal user, CancellationToken ct) =>
            {
                if (!user.IsInRole("Administrador")) return Results.Forbid();
                if (req.ReglaId != id) return Results.BadRequest(new { error = "Id mismatch" });
                var dto = await svc.UpsertAsync(req, ct);
                return Results.Ok(dto);
            });
            reglasGroup.MapPatch("/{id:int}/estado", async (int id, [FromBody] CambiarEstadoReglaRequest req, Infrastructure.Services.ReglaAdminService svc, ClaimsPrincipal user, CancellationToken ct) =>
            {
                if (!user.IsInRole("Administrador")) return Results.Forbid();
                await svc.CambiarEstadoAsync(id, req.Activa, req.RowVersion, ct);
                return Results.NoContent();
            });
            reglasGroup.MapDelete("/{id:int}", async (int id, [FromBody] byte[]? rowVersion, Infrastructure.Services.ReglaAdminService svc, ClaimsPrincipal user, CancellationToken ct) =>
            {
                if (!user.IsInRole("Administrador")) return Results.Forbid();
                await svc.EliminarAsync(id, rowVersion, ct);
                return Results.NoContent();
            });

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
                 app.Logger.LogInformation("?? Endpoint /api/motor/ejecutar llamado");
                 app.Logger.LogInformation($"Usuario: {user.Identity?.Name}, Autenticado: {user.Identity?.IsAuthenticated}");
                 app.Logger.LogInformation($"Roles: {string.Join(", ", user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

                 // Verificar roles permitidos
                 if (!user.IsInRole("Administrador") && !user.IsInRole("Coordinador"))
                 {
                     app.Logger.LogWarning("? Usuario sin permisos suficientes");
                     return Results.Forbid();
                 }

                 // Fecha de corte (default: hoy)
                 var fechaCorte = request.FechaCorte ?? DateTime.UtcNow;
                 var fechaCorteOnly = DateOnly.FromDateTime(fechaCorte);

                 app.Logger.LogInformation($"?? Ejecutando motor: FechaCorte={fechaCorteOnly}, ProgramaId={request.ProgramaId}, DryRun={request.DryRun}");

                 // Ejecutar motor
                 try
                 {
                     var resumen = await motor.EjecutarAsync(fechaCorteOnly, request.ProgramaId, request.DryRun, ct);

                     app.Logger.LogInformation($"? Motor ejecutado: Reglas={resumen.ReglasEjecutadas}, Alertas={resumen.AlertasGeneradas}, Errores={resumen.Errores}");

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

                     app.Logger.LogInformation($"?? Obtenidas {ultimasAlertas.Count} alertas de la BD");

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
                     app.Logger.LogError(ex, "? Error ejecutando motor");
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

            // ========================================
            // ENDPOINT PERSONALIZADO DE LOGIN CON 2FA
            // ========================================
            app.MapPost("/Account/PerformLogin", async (
                HttpContext context,
                [FromForm] string email,
                [FromForm] string password,
                [FromForm] bool? rememberMe,
                [FromForm] string? returnUrl,
                SignInManager<Usuario> signInManager,
                UserManager<Usuario> userManager,
                EmailTwoFactorService twoFactorService,
                EmailSenderService emailSender,
                ILogger<Program> logger) =>
            {
                logger.LogInformation("Intento de login para {Email}", email);

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    logger.LogWarning("Usuario no encontrado: {Email}", email);
                    return Results.Redirect($"/Account/Login?Message={Uri.EscapeDataString("Email o contraseña incorrectos")}");
                }

                // Verificar contraseña
                var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    logger.LogWarning("Usuario bloqueado: {Email}", email);
                    return Results.Redirect("/Account/Lockout");
                }

                if (!result.Succeeded)
                {
                    logger.LogWarning("Contraseña incorrecta para: {Email}", email);
                    return Results.Redirect($"/Account/Login?Message={Uri.EscapeDataString("Email o contraseña incorrectos")}");
                }

                // Si 2FA está habilitado, generar y enviar código
                if (user.TwoFactorEnabled)
                {
                    // Generar código de 6 caracteres
                    var code = twoFactorService.GenerateCode();
                    
                    // Almacenar código y sesión temporal
                    twoFactorService.StoreCode(email, code, user.Id, rememberMe ?? false);
                    
                    // Enviar código por email
                    await emailSender.SendPasswordResetCodeAsync(user, email, code);
                    
                    logger.LogInformation("Código 2FA enviado a {Email}", email);

                    return Results.Redirect($"/Account/VerifyTwoFactor?email={Uri.EscapeDataString(email)}&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
                }

                await signInManager.SignInAsync(user, isPersistent: rememberMe ?? false);
                logger.LogInformation("Login exitoso para {Email}", email);
                
                return Results.Redirect(returnUrl ?? "/");
            })
            .DisableAntiforgery(); 

            
            app.MapPost("/Account/Complete2FA", async (
                HttpContext context,
                [FromForm] string email,
                [FromForm] string code,
                [FromForm] string? returnUrl,
                SignInManager<Usuario> signInManager,
                UserManager<Usuario> userManager,
                EmailTwoFactorService twoFactorService,
                ILogger<Program> logger) =>
            {
                logger.LogInformation("Verificando código 2FA para {Email}", email);

                // Verificar que existe una sesión válida
                var session = twoFactorService.GetUserSession(email);
                if (session == null)
                {
                    logger.LogWarning("No hay sesión 2FA válida para {Email}", email);
                    return Results.Redirect($"/Account/Login?Message={Uri.EscapeDataString("Sesión expirada. Por favor, inicia sesión nuevamente.")}");
                }

                // Verificar el código
                if (!twoFactorService.VerifyCode(email, code))
                {
                    logger.LogWarning("Código 2FA incorrecto para {Email}", email);
                    var errorReturnUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
                    return Results.Redirect($"/Account/VerifyTwoFactor?email={Uri.EscapeDataString(email)}&returnUrl={Uri.EscapeDataString(errorReturnUrl)}&error={Uri.EscapeDataString("Código incorrecto o expirado")}");
                }

                // Cargar usuario
                var user = await userManager.FindByIdAsync(session.UserId.ToString());
                if (user == null)
                {
                    logger.LogError("Usuario no encontrado: {UserId}", session.UserId);
                    twoFactorService.ClearSession(email);
                    return Results.Redirect($"/Account/Login?Message={Uri.EscapeDataString("Error al completar el login")}");
                }

                // Limpiar sesión temporal
                twoFactorService.ClearSession(email);

                // Iniciar sesión real
                await signInManager.SignInAsync(user, isPersistent: session.RememberMe);
                
                logger.LogInformation("Usuario {Email} completó 2FA exitosamente", email);

                // Redirigir al destino (asegurar que nunca sea null o vacío)
                var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                return Results.Redirect(finalReturnUrl);
            })
            .DisableAntiforgery();

            app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();


            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            // ========================================
            // MANEJO DE RUTAS NO ENCONTRADAS (404)
            // ========================================
            // Este debe ser el ÚLTIMO MapFallback para capturar todas las rutas no manejadas
            app.MapFallback(async (HttpContext context) =>
            {
                // Excluir rutas de API y archivos estáticos
                if (context.Request.Path.StartsWithSegments("/api") ||
                    context.Request.Path.StartsWithSegments("/_framework") ||
                    context.Request.Path.StartsWithSegments("/_content") ||
                    context.Request.Path.Value?.Contains('.') == true) // Archivos estáticos
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                // Guardar la ruta original
                context.Items["OriginalPath"] = context.Request.Path.Value;
                
                // Redirigir a la página NotFound
                context.Response.Redirect("/NotFound");
            });

            app.Run();
        }
    }
}
