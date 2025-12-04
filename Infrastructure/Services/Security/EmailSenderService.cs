using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Security;

/// <summary>
/// Servicio de envío de emails para Identity con soporte para 2FA
/// </summary>
public class EmailSenderService : IEmailSender<Usuario>, IEmailSender
{
    private readonly ILogger<EmailSenderService> _logger;
    private readonly IConfiguration _configuration;

    public EmailSenderService(
        ILogger<EmailSenderService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtiene la URL base del servidor desde la configuración
    /// </summary>
    private string GetBaseUrl()
    {
        return _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7001";
    }

    /// <summary>
    /// Obtiene la URL del logo de la institución
    /// </summary>
    private string GetLogoUrl()
    {
        // Logo oficial de la ONG desde el sitio web institucional
        return "https://www.jpcbolivia.org/assets/images/logo.svg";
    }

    /// <summary>
    /// Genera el encabezado HTML común para todos los emails
    /// </summary>
    private string GenerarEncabezado(string titulo)
    {
        var logoUrl = GetLogoUrl();
        return $@"
            <!DOCTYPE html>
            <html lang='es'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            </head>
            <body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f5f5f5;'>
                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f5f5f5; padding: 40px 20px;'>
                    <tr>
                        <td align='center'>
                            <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 24px rgba(0,0,0,0.1);'>
                                <!-- Header con Logo -->
                                <tr>
                                    <td style='background-color: #4D3935; padding: 30px 40px; text-align: center;'>
                                        <img src='{logoUrl}' alt='ONG Juventud Sin Límites' style='max-width: 180px; height: auto; margin-bottom: 15px;' />
                                        <h1 style='color: #ffffff !important; font-size: 22px; font-weight: 600; margin: 0; letter-spacing: 0.5px; text-shadow: 1px 1px 2px rgba(0,0,0,0.3);'>{titulo}</h1>
                                    </td>
                                </tr>";
    }

    /// <summary>
    /// Genera el pie de página HTML común para todos los emails
    /// </summary>
    private string GenerarPiePagina()
    {
        return $@"
                                <!-- Footer -->
                                <tr>
                                    <td style='background-color: #4D3935; padding: 25px 40px; text-align: center;'>
                                        <p style='color: #9FD996; font-size: 14px; font-weight: 600; margin: 0 0 10px 0;'>
                                            ONG Juventud Sin Límites
                                        </p>
                                        <p style='color: #FEFEFD; font-size: 12px; margin: 0; opacity: 0.8;'>
                                            Sistema de Gestión Institucional
                                        </p>
                                        <hr style='border: none; border-top: 1px solid rgba(159, 217, 150, 0.3); margin: 15px 0;' />
                                        <p style='color: #FEFEFD; font-size: 11px; margin: 0; opacity: 0.6;'>
                                            Este es un correo automático. Por favor, no responda a este mensaje.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
    }

    /// <summary>
    /// Genera un botón de acción estilizado
    /// </summary>
    private string GenerarBoton(string texto, string url, string colorFondo = "#9FD996", string colorTexto = "#4D3935")
    {
        return $@"
            <table cellpadding='0' cellspacing='0' style='margin: 25px auto;'>
                <tr>
                    <td style='background: {colorFondo}; border-radius: 8px; box-shadow: 0 4px 12px rgba(159, 217, 150, 0.3);'>
                        <a href='{url}' style='display: inline-block; padding: 16px 40px; color: {colorTexto}; text-decoration: none; font-weight: 600; font-size: 16px;'>
                            {texto}
                        </a>
                    </td>
                </tr>
            </table>";
    }

    /// <summary>
    /// Envía email de confirmación de cuenta
    /// </summary>
    public Task SendConfirmationLinkAsync(Usuario user, string email, string confirmationLink)
    {
        var subject = "Confirma tu cuenta - ONG Juventud Sin Límites";
        var htmlMessage = GenerarEncabezado("Bienvenido a Nuestro Sistema") + $@"
                                <!-- Contenido -->
                                <tr>
                                    <td style='padding: 40px;'>
                                        <h2 style='color: #4D3935; font-size: 24px; margin: 0 0 20px 0;'>
                                            Hola, {user.Persona?.Nombres ?? "Usuario"}
                                        </h2>
                                        <p style='color: #6D534F; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                            Gracias por registrarte en el Sistema de Gestión de la ONG Juventud Sin Límites. 
                                            Para comenzar a utilizar tu cuenta, por favor confirma tu dirección de correo electrónico.
                                        </p>
                                        
                                        {GenerarBoton("Confirmar Mi Cuenta", confirmationLink)}
                                        
                                        <div style='background-color: #f8f9fa; border-radius: 8px; padding: 20px; margin-top: 25px;'>
                                            <p style='color: #6D534F; font-size: 14px; margin: 0;'>
                                                <strong style='color: #4D3935;'>Nota de seguridad:</strong> Si no solicitaste esta cuenta, 
                                                puedes ignorar este correo de forma segura. Tu información está protegida.
                                            </p>
                                        </div>
                                    </td>
                                </tr>
                " + GenerarPiePagina();

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Envía email para resetear contraseña
    /// </summary>
    public Task SendPasswordResetLinkAsync(Usuario user, string email, string resetLink)
    {
        var subject = "Recuperación de Contraseña - ONG Juventud Sin Límites";
        var htmlMessage = GenerarEncabezado("Recuperación de Contraseña") + $@"
                                <!-- Contenido -->
                                <tr>
                                    <td style='padding: 40px;'>
                                        <h2 style='color: #4D3935; font-size: 24px; margin: 0 0 20px 0;'>
                                            Hola, {user.Persona?.Nombres ?? "Usuario"}
                                        </h2>
                                        <p style='color: #6D534F; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                            Hemos recibido una solicitud para restablecer la contraseña de tu cuenta. 
                                            Haz clic en el botón de abajo para crear una nueva contraseña.
                                        </p>
                                        
                                        {GenerarBoton("Restablecer Contraseña", resetLink, "#F7C484", "#4D3935")}
                                        
                                        <!-- Alerta de tiempo -->
                                        <table width='100%' cellpadding='0' cellspacing='0' style='margin: 25px 0;'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #FFF3CD 0%, #FFEEBA 100%); border-left: 4px solid #F7C484; border-radius: 0 8px 8px 0; padding: 15px 20px;'>
                                                    <p style='color: #4D3935; font-size: 14px; margin: 0; font-weight: 500;'>
                                                        Este enlace expirará en 24 horas por motivos de seguridad.
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        <div style='background-color: #f8f9fa; border-radius: 8px; padding: 20px;'>
                                            <p style='color: #6D534F; font-size: 14px; margin: 0;'>
                                                <strong style='color: #4D3935;'>¿No solicitaste este cambio?</strong><br/>
                                                Si no solicitaste restablecer tu contraseña, puedes ignorar este correo. 
                                                Tu contraseña actual permanecerá sin cambios.
                                            </p>
                                        </div>
                                    </td>
                                </tr>
                " + GenerarPiePagina();

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Envía código de verificación de dos factores
    /// </summary>
    public Task SendPasswordResetCodeAsync(Usuario user, string email, string resetCode)
    {
        var subject = "Código de Verificación - ONG Juventud Sin Límites";
        var htmlMessage = GenerarEncabezado("Verificación de Identidad") + $@"
                                <!-- Contenido -->
                                <tr>
                                    <td style='padding: 40px;'>
                                        <h2 style='color: #4D3935; font-size: 24px; margin: 0 0 20px 0;'>
                                            Hola, {user.Persona?.Nombres ?? "Usuario"}
                                        </h2>
                                        <p style='color: #6D534F; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;'>
                                            Has solicitado iniciar sesión en el Sistema de Gestión. Para completar 
                                            el proceso de autenticación, ingresa el siguiente código de verificación:
                                        </p>
                                        
                                        <!-- Código de verificación -->
                                        <table width='100%' cellpadding='0' cellspacing='0' style='margin: 30px 0;'>
                                            <tr>
                                                <td align='center'>
                                                    <div style='background: linear-gradient(135deg, #9FD996 0%, #85C97C 100%); 
                                                                border-radius: 12px; 
                                                                padding: 25px 40px; 
                                                                display: inline-block;
                                                                box-shadow: 0 4px 16px rgba(159, 217, 150, 0.3);'>
                                                        <span style='font-size: 42px; 
                                                                     font-weight: 700; 
                                                                     letter-spacing: 12px; 
                                                                     color: #4D3935;
                                                                     font-family: ""Courier New"", monospace;'>
                                                            {resetCode}
                                                        </span>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        <!-- Alerta de tiempo -->
                                        <table width='100%' cellpadding='0' cellspacing='0' style='margin: 25px 0;'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #FFF3CD 0%, #FFEEBA 100%); border-left: 4px solid #F7C484; border-radius: 0 8px 8px 0; padding: 15px 20px;'>
                                                    <p style='color: #4D3935; font-size: 14px; margin: 0; font-weight: 500;'>
                                                        Este código expirará en 10 minutos.
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        <!-- Información de seguridad -->
                                        <div style='background-color: #f8f9fa; border-radius: 8px; padding: 20px; margin-top: 20px;'>
                                            <h4 style='color: #4D3935; font-size: 14px; margin: 0 0 12px 0; font-weight: 600;'>
                                                Información de Seguridad
                                            </h4>
                                            <ul style='color: #6D534F; font-size: 13px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                <li>Ingresa este código en la pantalla de verificación</li>
                                                <li>El código distingue entre mayúsculas y minúsculas</li>
                                                <li>Solo tienes 5 intentos para ingresarlo correctamente</li>
                                            </ul>
                                        </div>
                                        
                                        <!-- Alerta de seguridad -->
                                        <div style='background: linear-gradient(135deg, #FFEBEE 0%, #FFCDD2 100%); border-left: 4px solid #EF5350; border-radius: 0 8px 8px 0; padding: 15px 20px; margin-top: 20px;'>
                                            <p style='color: #C62828; font-size: 14px; margin: 0;'>
                                                <strong>¿No fuiste tú?</strong> Si no intentaste iniciar sesión, 
                                                ignora este correo y considera cambiar tu contraseña de inmediato.
                                            </p>
                                        </div>
                                    </td>
                                </tr>
                " + GenerarPiePagina();

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Envía credenciales de nuevo usuario (contraseña temporal)
    /// </summary>
    public Task SendNewUserCredentialsAsync(Usuario user, string email, string tempPassword, string rol)
    {
        var subject = "Bienvenido - Credenciales de Acceso al Sistema";
        var htmlMessage = GenerarEncabezado("Bienvenido al Sistema") + $@"
                                <!-- Contenido -->
                                <tr>
                                    <td style='padding: 40px;'>
                                        <h2 style='color: #4D3935; font-size: 24px; margin: 0 0 20px 0;'>
                                            Hola, {user.Persona?.Nombres ?? "Usuario"}
                                        </h2>
                                        <p style='color: #6D534F; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;'>
                                            Se ha creado una cuenta para ti en el Sistema de Gestión de la 
                                            ONG Juventud Sin Límites. A continuación encontrarás tus credenciales de acceso.
                                        </p>
                                        
                                        <!-- Credenciales -->
                                        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f8f9fa; border-radius: 12px; overflow: hidden; margin: 25px 0;'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #4D3935 0%, #6D534F 100%); padding: 15px 20px;'>
                                                    <h3 style='color: #FEFEFD; font-size: 16px; margin: 0; font-weight: 600;'>
                                                        Credenciales de Acceso
                                                    </h3>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 25px;'>
                                                    <table width='100%' cellpadding='0' cellspacing='0'>
                                                        <tr>
                                                            <td style='padding: 10px 0; color: #6D534F; font-size: 14px;'>Usuario (Email):</td>
                                                            <td style='padding: 10px 0; color: #4D3935; font-weight: 600; font-size: 14px;'>{email}</td>
                                                        </tr>
                                                        <tr>
                                                            <td style='padding: 10px 0; color: #6D534F; font-size: 14px;'>Contraseña Temporal:</td>
                                                            <td style='padding: 10px 0;'>
                                                                <span style='background: linear-gradient(135deg, #F7C484 0%, #F3C95A 100%); 
                                                                             padding: 8px 16px; 
                                                                             border-radius: 6px; 
                                                                             font-family: ""Courier New"", monospace;
                                                                             font-size: 16px; 
                                                                             font-weight: 700; 
                                                                             color: #4D3935; 
                                                                             letter-spacing: 1px;'>
                                                                    {tempPassword}
                                                                </span>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style='padding: 10px 0; color: #6D534F; font-size: 14px;'>Rol Asignado:</td>
                                                            <td style='padding: 10px 0;'>
                                                                <span style='background-color: #9FD996; padding: 4px 12px; border-radius: 4px; color: #4D3935; font-weight: 600; font-size: 13px;'>
                                                                    {rol}
                                                                </span>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        <!-- Alerta de seguridad -->
                                        <table width='100%' cellpadding='0' cellspacing='0' style='margin: 25px 0;'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #FFF3CD 0%, #FFEEBA 100%); border-left: 4px solid #F7C484; border-radius: 0 8px 8px 0; padding: 20px;'>
                                                    <h4 style='color: #4D3935; font-size: 14px; margin: 0 0 10px 0; font-weight: 600;'>
                                                        Importante - Seguridad
                                                    </h4>
                                                    <ul style='color: #6D534F; font-size: 13px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                        <li><strong>Debes cambiar esta contraseña</strong> en tu primer inicio de sesión</li>
                                                        <li>Esta es una contraseña temporal de un solo uso</li>
                                                        <li>No compartas estas credenciales con nadie</li>
                                                        <li>El sistema tiene autenticación de dos factores (2FA) habilitada</li>
                                                    </ul>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        {GenerarBoton("Iniciar Sesión Ahora", $"{GetBaseUrl()}/Account/Login")}
                                        
                                        <!-- Pasos -->
                                        <div style='background-color: #E8F5E9; border-radius: 8px; padding: 20px; margin-top: 25px;'>
                                            <h4 style='color: #4D3935; font-size: 14px; margin: 0 0 12px 0; font-weight: 600;'>
                                                Pasos para Ingresar
                                            </h4>
                                            <ol style='color: #6D534F; font-size: 13px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                <li>Haz clic en el botón ""Iniciar Sesión Ahora""</li>
                                                <li>Ingresa tu email y la contraseña temporal</li>
                                                <li>Verifica tu identidad con el código 2FA enviado a tu email</li>
                                                <li>Crea una nueva contraseña segura</li>
                                                <li>¡Listo! Ya podrás usar el sistema</li>
                                            </ol>
                                        </div>
                                    </td>
                                </tr>
                " + GenerarPiePagina();

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Envía email de reset de contraseña con credenciales nuevas
    /// </summary>
    public Task SendPasswordResetWithCredentialsAsync(Usuario user, string email, string newPassword, string rol)
    {
        var subject = "Contraseña Reseteada - Nueva Contraseña Temporal";
        var htmlMessage = GenerarEncabezado("Contraseña Reseteada") + $@"
                                <!-- Contenido -->
                                <tr>
                                    <td style='padding: 40px;'>
                                        <h2 style='color: #4D3935; font-size: 24px; margin: 0 0 20px 0;'>
                                            Hola, {user.Persona?.Nombres ?? "Usuario"}
                                        </h2>
                                        <p style='color: #6D534F; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;'>
                                            Tu contraseña ha sido reseteada por un administrador del sistema. 
                                            A continuación encontrarás tu nueva contraseña temporal.
                                        </p>
                                        
                                        <!-- Nueva contraseña -->
                                        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f8f9fa; border-radius: 12px; overflow: hidden; margin: 25px 0;'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #4D3935 0%, #6D534F 100%); padding: 15px 20px;'>
                                                    <h3 style='color: #FEFEFD; font-size: 16px; margin: 0; font-weight: 600;'>
                                                        Nueva Contraseña Temporal
                                                    </h3>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 25px; text-align: center;'>
                                                    <div style='background: linear-gradient(135deg, #F7C484 0%, #F3C95A 100%); 
                                                                border-radius: 8px; 
                                                                padding: 20px 30px; 
                                                                display: inline-block;'>
                                                        <span style='font-family: ""Courier New"", monospace;
                                                                     font-size: 24px; 
                                                                     font-weight: 700; 
                                                                     color: #4D3935; 
                                                                     letter-spacing: 3px;'>
                                                            {newPassword}
                                                        </span>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        <!-- Alerta de seguridad -->
                                        <table width='100%' cellpadding='0' cellspacing='0' style='margin: 25px 0;'>
                                            <tr>
                                                <td style='background: linear-gradient(135deg, #FFF3CD 0%, #FFEEBA 100%); border-left: 4px solid #F7C484; border-radius: 0 8px 8px 0; padding: 20px;'>
                                                    <h4 style='color: #4D3935; font-size: 14px; margin: 0 0 10px 0; font-weight: 600;'>
                                                        Importante - Seguridad
                                                    </h4>
                                                    <ul style='color: #6D534F; font-size: 13px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                        <li><strong>Debes cambiar esta contraseña</strong> en tu próximo inicio de sesión</li>
                                                        <li>Esta es una contraseña temporal de un solo uso</li>
                                                        <li>No compartas esta contraseña con nadie</li>
                                                        <li>Si no solicitaste este cambio, contacta al administrador inmediatamente</li>
                                                    </ul>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        {GenerarBoton("Iniciar Sesión", $"{GetBaseUrl()}/Account/Login")}
                                        
                                        <!-- Recordatorio -->
                                        <div style='background-color: #E8F5E9; border-radius: 8px; padding: 20px; margin-top: 25px;'>
                                            <h4 style='color: #4D3935; font-size: 14px; margin: 0 0 12px 0; font-weight: 600;'>
                                                Recuerda
                                            </h4>
                                            <ul style='color: #6D534F; font-size: 13px; margin: 0; padding-left: 20px; line-height: 1.8;'>
                                                <li>Usuario: <strong>{email}</strong></li>
                                                <li>Contraseña: <strong>La mostrada arriba</strong></li>
                                                <li>Después del login, se te pedirá crear una nueva contraseña segura</li>
                                            </ul>
                                        </div>
                                    </td>
                                </tr>
                " + GenerarPiePagina();

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Método genérico para enviar emails usando SMTP
    /// </summary>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            // Obtener configuración de SMTP
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"] ?? "ONG Juventud Sin Límites";
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            // Validar configuración
            if (string.IsNullOrEmpty(smtpServer) || 
                string.IsNullOrEmpty(senderEmail) || 
                string.IsNullOrEmpty(username) || 
                string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Configuración de email incompleta. Email no enviado.");
                _logger.LogInformation("===== EMAIL NO ENVIADO (Configuración faltante) =====");
                _logger.LogInformation("Para: {Email}", email);
                _logger.LogInformation("Asunto: {Subject}", subject);
                _logger.LogInformation("======================================================");
                return;
            }

            // Crear mensaje
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Enviar email
            using (var client = new SmtpClient())
            {
                // Conectar al servidor SMTP
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);

                // Autenticar
                await client.AuthenticateAsync(username, password);

                // Enviar
                await client.SendAsync(message);

                // Desconectar
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation("Email enviado exitosamente a {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {Email}", email);
        }
    }

    // Implementación de IEmailSender (sin tipo genérico)
    Task IEmailSender.SendEmailAsync(string email, string subject, string message)
    {
        return SendEmailAsync(email, subject, message);
    }
}
