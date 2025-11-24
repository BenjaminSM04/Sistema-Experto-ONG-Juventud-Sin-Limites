using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Account.Pages;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Account.Pages.Manage;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Microsoft.AspNetCore.Routing
{
    internal static class IdentityComponentsEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/Account");

            // NOTA: El endpoint /PerformLogin está implementado en Program.cs con soporte para 2FA
            // No registrar aquí para evitar duplicados

            /*
            // Endpoint para manejar el login desde Blazor Server
            accountGroup.MapPost("/PerformLogin", async (
                HttpContext context,
                [FromServices] SignInManager<Usuario> signInManager,
                [FromServices] ILogger<Login> logger,
                [FromForm] string? email,
                [FromForm] string? password,
                [FromForm] bool rememberMe = false,
                [FromForm] string? returnUrl = null) =>
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return Results.Redirect("/Account/Login?Message=" + Uri.EscapeDataString("Error: Email y contraseña son requeridos."));
                }

                var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    logger.LogInformation("User {Email} logged in.", email);
                    // Redirigir al Home por defecto
                    var redirectUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                    return Results.Redirect(redirectUrl);
                }
                else if (result.RequiresTwoFactor)
                {
                    var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "" : returnUrl;
                    var twoFactorUrl = $"/Account/LoginWith2fa?returnUrl={Uri.EscapeDataString(safeReturnUrl)}&rememberMe={rememberMe}";
                    return Results.Redirect(twoFactorUrl);
                }
                else if (result.IsLockedOut)
                {
                    logger.LogWarning("User {Email} account locked out.", email);
                    return Results.Redirect("/Account/Lockout");
                }
                else
                {
                    return Results.Redirect("/Account/Login?Message=" + Uri.EscapeDataString("Error: Credenciales inválidas. Por favor, verifica tu email y contraseña."));
                }
            })
            .DisableAntiforgery();
            */

            accountGroup.MapPost("/PerformExternalLogin", (
                HttpContext context,
                [FromServices] SignInManager<Usuario> signInManager,
                [FromForm] string provider,
                [FromForm] string returnUrl) =>
            {
                IEnumerable<KeyValuePair<string, StringValues>> query = [
                         new("ReturnUrl", returnUrl),
                       new("Action", ExternalLogin.LoginCallbackAction)];

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/ExternalLogin",
                    QueryString.Create(query));

                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return TypedResults.Challenge(properties, [provider]);
            });

            accountGroup.MapPost("/Logout", async (
                ClaimsPrincipal user,
                SignInManager<Usuario> signInManager,
                [FromForm] string returnUrl) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.LocalRedirect($"~/{returnUrl}");
            })
            .DisableAntiforgery(); // Deshabilitar validación antiforgery para logout

            var manageGroup = accountGroup.MapGroup("/Manage").RequireAuthorization();

            manageGroup.MapPost("/LinkExternalLogin", async (
                HttpContext context,
                [FromServices] SignInManager<Usuario> signInManager,
                [FromForm] string provider) =>
            {
                await context.SignOutAsync(IdentityConstants.ExternalScheme);

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/Manage/ExternalLogins",
                    QueryString.Create("Action", ExternalLogins.LinkLoginCallbackAction));

                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, signInManager.UserManager.GetUserId(context.User));
                return TypedResults.Challenge(properties, [provider]);
            });

            var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");

            manageGroup.MapPost("/DownloadPersonalData", async (
                HttpContext context,
                [FromServices] UserManager<Usuario> userManager,
                [FromServices] AuthenticationStateProvider authenticationStateProvider) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user is null)
                {
                    return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
                }

                var userId = await userManager.GetUserIdAsync(user);
                downloadLogger.LogInformation("User with ID '{UserId}' asked for their personal data.", userId);

                var personalData = new Dictionary<string, string>();
                var personalDataProps = typeof(Usuario).GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
                }

                var logins = await userManager.GetLoginsAsync(user);
                foreach (var l in logins)
                {
                    personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
                }

                personalData.Add("Authenticator Key", (await userManager.GetAuthenticatorKeyAsync(user))!);
                var fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);

                context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
                return TypedResults.File(fileBytes, contentType: "application/json", fileDownloadName: "PersonalData.json");
            });

            return accountGroup;
        }
    }
}
