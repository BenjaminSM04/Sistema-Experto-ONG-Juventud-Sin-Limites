using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Components;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Account;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Data;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Extensions;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Interceptors;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Inference;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
            builder.Services.AddScoped<IFeatureProvider, FeatureProvider>();
            builder.Services.AddScoped<IMotorInferencia, MotorInferencia>();
            builder.Services.AddHostedService<MotorScheduler>();


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
             options.Password.RequireDigit = true;
             options.Password.RequireLowercase = true;
             options.Password.RequireUppercase = true;
             options.Password.RequireNonAlphanumeric = true;
             options.Password.RequiredLength = 8;
             options.User.RequireUniqueEmail = true;
         })
     .AddRoles<Rol>() // Agregar soporte para roles personalizados
   .AddEntityFrameworkStores<ApplicationDbContext>()
     .AddSignInManager()
      .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<Usuario>, IdentityNoOpEmailSender>();
            builder.Services.AddMudServices();

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

            app.MapRazorComponents<App>()
         .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
