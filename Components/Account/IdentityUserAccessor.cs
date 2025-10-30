using Microsoft.AspNetCore.Identity;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Components.Account
{
    internal sealed class IdentityUserAccessor(UserManager<Usuario> userManager)
    {
        public async Task<Usuario> GetRequiredUserAsync(HttpContext context)
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user is null)
            {
                // En lugar de redirigir, lanzamos una excepción
                throw new InvalidOperationException($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
            }

            return user;
        }
    }
}
