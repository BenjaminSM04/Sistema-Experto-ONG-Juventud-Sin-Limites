using Microsoft.AspNetCore.Identity;
using Sistema_Experto_ONG_Juventud_Sin_Limites.Domain.Security;
using System.Security.Cryptography;
using System.Text;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Services.Security;

/// <summary>
/// Servicio para autenticación de dos factores vía email
/// </summary>
public class EmailTwoFactorService
{
    private readonly ILogger<EmailTwoFactorService> _logger;
    private readonly Dictionary<string, TwoFactorCode> _codes = new();
    private readonly Dictionary<string, TwoFactorUserSession> _sessions = new();
    private const int CodeExpirationMinutes = 10;
    private const int CodeLength = 6;

    public EmailTwoFactorService(ILogger<EmailTwoFactorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Genera un código alfanumérico de 6 caracteres
    /// </summary>
    public string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Sin caracteres confusos (0,O,1,I)
        var code = new char[CodeLength];
        
        for (int i = 0; i < CodeLength; i++)
        {
            code[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        
        return new string(code);
    }

    /// <summary>
    /// Almacena el código generado para un usuario y crea una sesión temporal
    /// </summary>
    public void StoreCode(string userEmail, string code, int userId, bool rememberMe)
    {
        // Limpiar códigos expirados
        CleanExpiredCodes();

        var emailLower = userEmail.ToLower();

        _codes[emailLower] = new TwoFactorCode
        {
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(CodeExpirationMinutes),
            Attempts = 0
        };

        _sessions[emailLower] = new TwoFactorUserSession
        {
            UserId = userId,
            Email = userEmail,
            RememberMe = rememberMe,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(CodeExpirationMinutes)
        };

        _logger.LogInformation("Código 2FA generado para {Email}, expira en {Minutes} minutos", 
            userEmail, CodeExpirationMinutes);
    }

    /// <summary>
    /// Obtiene la sesión temporal del usuario para 2FA
    /// </summary>
    public TwoFactorUserSession? GetUserSession(string userEmail)
    {
        var email = userEmail.ToLower();

        if (!_sessions.ContainsKey(email))
            return null;

        var session = _sessions[email];

        // Verificar si la sesión expiró
        if (DateTime.UtcNow > session.ExpiresAt)
        {
            _sessions.Remove(email);
            return null;
        }

        return session;
    }

    /// <summary>
    /// Verifica si el código proporcionado es válido
    /// </summary>
    public bool VerifyCode(string userEmail, string providedCode)
    {
        var email = userEmail.ToLower();

        if (!_codes.ContainsKey(email))
        {
            _logger.LogWarning("Intento de verificación para email sin código: {Email}", userEmail);
            return false;
        }

        var storedCode = _codes[email];

        // Verificar si el código expiró
        if (DateTime.UtcNow > storedCode.ExpiresAt)
        {
            _logger.LogWarning("Código expirado para {Email}", userEmail);
            _codes.Remove(email);
            _sessions.Remove(email);
            return false;
        }

        // Incrementar intentos
        storedCode.Attempts++;

        // Bloquear después de 5 intentos
        if (storedCode.Attempts > 5)
        {
            _logger.LogWarning("Demasiados intentos fallidos para {Email}", userEmail);
            _codes.Remove(email);
            _sessions.Remove(email);
            return false;
        }

        // Verificar código (insensible a mayúsculas/minúsculas)
        if (string.Equals(storedCode.Code, providedCode, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Código 2FA verificado exitosamente para {Email}", userEmail);
            _codes.Remove(email); // Remover código usado
            return true;
        }

        _logger.LogWarning("Código incorrecto para {Email}. Intento {Attempt}/5", 
            userEmail, storedCode.Attempts);
        return false;
    }

    /// <summary>
    /// Limpia la sesión después de completar el login
    /// </summary>
    public void ClearSession(string userEmail)
    {
        var email = userEmail.ToLower();
        _codes.Remove(email);
        _sessions.Remove(email);
    }

    /// <summary>
    /// Obtiene el tiempo restante antes de que expire el código
    /// </summary>
    public TimeSpan? GetTimeRemaining(string userEmail)
    {
        var email = userEmail.ToLower();

        if (!_codes.ContainsKey(email))
            return null;

        var remaining = _codes[email].ExpiresAt - DateTime.UtcNow;
        return remaining.TotalSeconds > 0 ? remaining : null;
    }

    /// <summary>
    /// Elimina el código de un usuario
    /// </summary>
    public void RemoveCode(string userEmail)
    {
        var email = userEmail.ToLower();
        _codes.Remove(email);
        _sessions.Remove(email);
    }

    /// <summary>
    /// Limpia códigos expirados del diccionario
    /// </summary>
    private void CleanExpiredCodes()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _codes.Where(kvp => now > kvp.Value.ExpiresAt)
                                .Select(kvp => kvp.Key)
                                .ToList();

        foreach (var key in expiredKeys)
        {
            _codes.Remove(key);
            _sessions.Remove(key);
        }

        if (expiredKeys.Any())
        {
            _logger.LogDebug("Eliminados {Count} códigos expirados", expiredKeys.Count);
        }
    }

    private class TwoFactorCode
    {
        public string Code { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; }
    }

    public class TwoFactorUserSession
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public bool RememberMe { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
