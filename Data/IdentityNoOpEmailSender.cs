using Microsoft.AspNetCore.Identity;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Data;

/// <summary>
/// Implementación temporal del IEmailSender para Usuario
/// TODO: Reemplazar con implementación real de envío de emails
/// </summary>
internal sealed class IdentityNoOpEmailSender : IEmailSender<Usuario>
{
    private readonly ILogger<IdentityNoOpEmailSender> _logger;

    public IdentityNoOpEmailSender(ILogger<IdentityNoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendConfirmationLinkAsync(Usuario user, string email, string confirmationLink)
    {
        _logger.LogInformation("Confirmation link for {Email}: {ConfirmationLink}", email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(Usuario user, string email, string resetLink)
    {
        _logger.LogInformation("Password reset link for {Email}: {ResetLink}", email, resetLink);
      return Task.CompletedTask;
 }

    public Task SendPasswordResetCodeAsync(Usuario user, string email, string resetCode)
    {
        _logger.LogInformation("Password reset code for {Email}: {ResetCode}", email, resetCode);
      return Task.CompletedTask;
    }
}
