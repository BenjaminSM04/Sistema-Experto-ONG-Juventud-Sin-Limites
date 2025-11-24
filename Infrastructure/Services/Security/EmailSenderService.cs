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
    /// Envía email de confirmación de cuenta
    /// </summary>
    public Task SendConfirmationLinkAsync(Usuario user, string email, string confirmationLink)
    {
        var subject = "Confirma tu cuenta - ONG Juventud Sin Límites";
        var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(90deg, #4D3935 0%, #6D534F 100%); color: #FEFEFD; padding: 20px; text-align: center;'>
                    <h1>🎉 Bienvenido/a a ONG Juventud Sin Límites</h1>
                </div>
                <div style='padding: 30px; background: #FEFEFD;'>
                    <h2>Hola {user.Persona?.Nombres ?? ""}!</h2>
                    <p style='font-size: 16px; line-height: 1.6;'>
                        Gracias por registrarte en nuestro sistema. Para comenzar a usar tu cuenta, 
                        por favor confirma tu dirección de email haciendo clic en el botón de abajo:
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' 
                           style='background: linear-gradient(135deg, #9FD996 0%, #85C97C 100%); 
                                  color: #4D3935; 
                                  padding: 15px 40px; 
                                  text-decoration: none; 
                                  border-radius: 8px; 
                                  font-weight: 600;
                                  display: inline-block;'>
                            Confirmar Mi Cuenta
                        </a>
                    </div>
                    <p style='font-size: 14px; color: #6D534F;'>
                        Si no solicitaste esta cuenta, puedes ignorar este correo de forma segura.
                    </p>
                    <hr style='border: 1px solid #F7C484; margin: 30px 0;'>
                    <p style='font-size: 12px; color: #6D534F; text-align: center;'>
                        <strong>Sistema ONG Juventud Sin Límites</strong><br>
                        Este es un correo automático, por favor no responder.
                    </p>
                </div>
            </div>
        ";

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Envía email para resetear contraseña
    /// </summary>
    public Task SendPasswordResetLinkAsync(Usuario user, string email, string resetLink)
    {
        var subject = "Recuperación de Contraseña - ONG Juventud Sin Límites";
        var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(90deg, #4D3935 0%, #6D534F 100%); color: #FEFEFD; padding: 20px; text-align: center;'>
                    <h1>🔐 Recuperación de Contraseña</h1>
                </div>
                <div style='padding: 30px; background: #FEFEFD;'>
                    <h2>Hola {user.Persona?.Nombres ?? ""}!</h2>
                    <p style='font-size: 16px; line-height: 1.6;'>
                        Recibimos una solicitud para restablecer la contraseña de tu cuenta.
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' 
                           style='background: linear-gradient(135deg, #F7C484 0%, #F3C95A 100%); 
                                  color: #4D3935; 
                                  padding: 15px 40px; 
                                  text-decoration: none; 
                                  border-radius: 8px; 
                                  font-weight: 600;
                                  display: inline-block;'>
                            Restablecer Contraseña
                        </a>
                    </div>
                    <p style='font-size: 14px; color: #6D534F; background: #FFF3CD; padding: 15px; border-left: 4px solid #F7C484;'>
                        ⏰ <strong>Este enlace expirará en 24 horas.</strong>
                    </p>
                    <p style='font-size: 14px; color: #6D534F;'>
                        Si no solicitaste restablecer tu contraseña, puedes ignorar este correo de forma segura. 
                        Tu contraseña no será modificada.
                    </p>
                    <hr style='border: 1px solid #F7C484; margin: 30px 0;'>
                    <p style='font-size: 12px; color: #6D534F; text-align: center;'>
                        <strong>Sistema ONG Juventud Sin Límites</strong><br>
                        Este es un correo automático, por favor no responder.
                    </p>
                </div>
            </div>
        ";

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Envía código de verificación de dos factores
    /// </summary>
    public Task SendPasswordResetCodeAsync(Usuario user, string email, string resetCode)
    {
        var subject = "🔒 Código de Verificación 2FA - ONG Juventud Sin Límites";
        var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(90deg, #4D3935 0%, #6D534F 100%); color: #FEFEFD; padding: 20px; text-align: center;'>
                    <h1>🛡️ Verificación en Dos Factores</h1>
                </div>
                <div style='padding: 30px; background: #FEFEFD;'>
                    <h2>Hola {user.Persona?.Nombres ?? ""}!</h2>
                    <p style='font-size: 16px; line-height: 1.6;'>
                        Has solicitado iniciar sesión en el Sistema ONG Juventud Sin Límites. 
                        Para continuar, ingresa el siguiente código de verificación:
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <div style='background: linear-gradient(135deg, #9FD996 0%, #85C97C 100%); 
                                    padding: 30px; 
                                    border-radius: 12px; 
                                    display: inline-block;'>
                            <h1 style='font-size: 48px; 
                                       letter-spacing: 12px; 
                                       color: #4D3935; 
                                       margin: 0;
                                       font-weight: 700;
                                       text-shadow: 2px 2px 4px rgba(0,0,0,0.1);'>
                                {resetCode}
                            </h1>
                        </div>
                    </div>
                    <p style='font-size: 14px; color: #6D534F; background: #FFF3CD; padding: 15px; border-left: 4px solid #F7C484; text-align: center;'>
                        ⏰ <strong>Este código expirará en 10 minutos</strong>
                    </p>
                    <div style='background: #F7F7F7; padding: 20px; border-radius: 8px; margin-top: 20px;'>
                        <h3 style='color: #4D3935; margin-top: 0;'>ℹ️ Información de Seguridad:</h3>
                        <ul style='color: #6D534F; font-size: 14px;'>
                            <li>Ingresa este código en la pantalla de verificación</li>
                            <li>El código distingue entre mayúsculas y minúsculas</li>
                            <li>Solo tienes 5 intentos para ingresarlo correctamente</li>
                        </ul>
                    </div>
                    <p style='font-size: 14px; color: #dc3545; margin-top: 20px;'>
                        ⚠️ <strong>¿No fuiste tú?</strong> Si no intentaste iniciar sesión, 
                        ignora este correo y considera cambiar tu contraseña de inmediato.
                    </p>
                    <hr style='border: 1px solid #F7C484; margin: 30px 0;'>
                    <p style='font-size: 12px; color: #6D534F; text-align: center;'>
                        <strong>Sistema ONG Juventud Sin Límites</strong><br>
                        Este es un correo automático, por favor no responder.
                    </p>
                </div>
            </div>
        ";

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Envía credenciales de nuevo usuario (contraseña temporal)
    /// </summary>
    public Task SendNewUserCredentialsAsync(Usuario user, string email, string tempPassword, string rol)
    {
        var subject = "🎉 Bienvenido - Credenciales de Acceso al Sistema";
        var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(90deg, #4D3935 0%, #6D534F 100%); color: #FEFEFD; padding: 20px; text-align: center;'>
                    <h1>🎉 ¡Bienvenido/a al Sistema!</h1>
                </div>
                <div style='padding: 30px; background: #FEFEFD;'>
                    <h2>Hola {user.Persona?.Nombres ?? ""}!</h2>
                    <p style='font-size: 16px; line-height: 1.6;'>
                        Se ha creado una cuenta para ti en el <strong>Sistema ONG Juventud Sin Límites</strong>.
                        A continuación encontrarás tus credenciales de acceso:
                    </p>
                    
                    <div style='background: #F7F7F7; padding: 25px; border-radius: 8px; margin: 25px 0;'>
                        <h3 style='color: #4D3935; margin-top: 0;'>🔐 Tus Credenciales de Acceso</h3>
                        <table style='width: 100%; font-size: 14px;'>
                            <tr>
                                <td style='padding: 10px 0; color: #6D534F;'><strong>Usuario (Email):</strong></td>
                                <td style='padding: 10px 0; color: #4D3935; font-weight: 600;'>{email}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px 0; color: #6D534F;'><strong>Contraseña Temporal:</strong></td>
                                <td style='padding: 10px 0; background: linear-gradient(135deg, #F7C484 0%, #F3C95A 100%); 
                                           border-radius: 4px; text-align: center;'>
                                    <code style='font-size: 18px; font-weight: 700; color: #4D3935; letter-spacing: 2px;'>{tempPassword}</code>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding: 10px 0; color: #6D534F;'><strong>Rol Asignado:</strong></td>
                                <td style='padding: 10px 0; color: #4D3935; font-weight: 600;'>{rol}</td>
                            </tr>
                        </table>
                    </div>

                    <div style='background: #FFF3CD; padding: 20px; border-left: 4px solid #F7C484; margin: 25px 0;'>
                        <h4 style='color: #4D3935; margin-top: 0;'>⚠️ ¡IMPORTANTE - Seguridad!</h4>
                        <ul style='color: #6D534F; font-size: 14px; margin-bottom: 0;'>
                            <li><strong>Debes cambiar esta contraseña</strong> en tu primer inicio de sesión</li>
                            <li>Esta es una contraseña temporal de un solo uso</li>
                            <li>No compartas estas credenciales con nadie</li>
                            <li>El sistema tiene autenticación de dos factores (2FA) habilitada para mayor seguridad</li>
                        </ul>
                    </div>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='https://localhost:7001/Account/Login' 
                           style='background: linear-gradient(135deg, #9FD996 0%, #85C97C 100%); 
                                  color: #4D3935; 
                                  padding: 15px 40px; 
                                  text-decoration: none; 
                                  border-radius: 8px; 
                                  font-weight: 600;
                                  display: inline-block;'>
                            Iniciar Sesión Ahora
                        </a>
                    </div>

                    <div style='background: #E8F5E9; padding: 20px; border-radius: 8px; margin-top: 25px;'>
                        <h4 style='color: #4D3935; margin-top: 0;'>📝 Pasos para Ingresar:</h4>
                        <ol style='color: #6D534F; font-size: 14px; margin-bottom: 0;'>
                            <li>Haz clic en el botón ""Iniciar Sesión Ahora""</li>
                            <li>Ingresa tu email y la contraseña temporal</li>
                            <li>El sistema te pedirá verificar tu identidad con un código 2FA enviado a tu email</li>
                            <li>Luego se te solicitará crear una nueva contraseña segura</li>
                            <li>¡Listo! Ya podrás usar el sistema</li>
                        </ol>
                    </div>

                    <hr style='border: 1px solid #F7C484; margin: 30px 0;'>
                    
                    <p style='font-size: 12px; color: #6D534F; text-align: center;'>
                        <strong>Sistema ONG Juventud Sin Límites</strong><br>
                        Si tienes problemas para acceder, contacta al administrador del sistema.<br>
                        Este es un correo automático, por favor no responder.
                    </p>
                </div>
            </div>
        ";

        return SendEmailAsync(email, subject, htmlMessage);
    }

    /// <summary>
    /// Envía email de reset de contraseña con credenciales nuevas
    /// </summary>
    public Task SendPasswordResetWithCredentialsAsync(Usuario user, string email, string newPassword, string rol)
    {
        var subject = "🔄 Contraseña Reseteada - Nueva Contraseña Temporal";
        var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(90deg, #4D3935 0%, #6D534F 100%); color: #FEFEFD; padding: 20px; text-align: center;'>
                    <h1>🔄 Contraseña Reseteada</h1>
                </div>
                <div style='padding: 30px; background: #FEFEFD;'>
                    <h2>Hola {user.Persona?.Nombres ?? ""}!</h2>
                    <p style='font-size: 16px; line-height: 1.6;'>
                        Tu contraseña ha sido reseteada por un administrador del sistema.
                        A continuación encontrarás tu <strong>nueva contraseña temporal</strong>:
                    </p>
                    
                    <div style='background: #F7F7F7; padding: 25px; border-radius: 8px; margin: 25px 0;'>
                        <h3 style='color: #4D3935; margin-top: 0;'>🔐 Nueva Contraseña Temporal</h3>
                        <div style='text-align: center; padding: 20px; background: linear-gradient(135deg, #F7C484 0%, #F3C95A 100%); 
                                    border-radius: 8px; margin: 15px 0;'>
                            <code style='font-size: 24px; font-weight: 700; color: #4D3935; letter-spacing: 3px;'>{newPassword}</code>
                        </div>
                    </div>

                    <div style='background: #FFF3CD; padding: 20px; border-left: 4px solid #F7C484; margin: 25px 0;'>
                        <h4 style='color: #4D3935; margin-top: 0;'>⚠️ ¡IMPORTANTE - Seguridad!</h4>
                        <ul style='color: #6D534F; font-size: 14px; margin-bottom: 0;'>
                            <li><strong>Debes cambiar esta contraseña</strong> en tu próximo inicio de sesión</li>
                            <li>Esta es una contraseña temporal de un solo uso</li>
                            <li>No compartas esta contraseña con nadie</li>
                            <li>Si no solicitaste este cambio, contacta al administrador inmediatamente</li>
                        </ul>
                    </div>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='https://localhost:7001/Account/Login' 
                           style='background: linear-gradient(135deg, #9FD996 0%, #85C97C 100%); 
                                  color: #4D3935; 
                                  padding: 15px 40px; 
                                  text-decoration: none; 
                                  border-radius: 8px; 
                                  font-weight: 600;
                                  display: inline-block;'>
                            Iniciar Sesión
                        </a>
                    </div>

                    <div style='background: #E8F5E9; padding: 20px; border-radius: 8px; margin-top: 25px;'>
                        <h4 style='color: #4D3935; margin-top: 0;'>📝 Recuerda:</h4>
                        <ul style='color: #6D534F; font-size: 14px; margin-bottom: 0;'>
                            <li>Usuario: <strong>{email}</strong></li>
                            <li>Contraseña: <strong>La mostrada arriba</strong></li>
                            <li>Después del login, se te pedirá crear una nueva contraseña segura</li>
                        </ul>
                    </div>

                    <hr style='border: 1px solid #F7C484; margin: 30px 0;'>
                    
                    <p style='font-size: 12px; color: #6D534F; text-align: center;'>
                        <strong>Sistema ONG Juventud Sin Límites</strong><br>
                        Si tienes problemas para acceder, contacta al administrador del sistema.<br>
                        Este es un correo automático, por favor no responder.
                    </p>
                </div>
            </div>
        ";

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
                _logger.LogWarning("⚠️ Configuración de email incompleta. Email no enviado.");
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
