using Microsoft.AspNetCore.Components.Forms;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Validation;

/// <summary>
/// Validador de archivos subidos para prevenir ataques y archivos maliciosos
/// </summary>
public class FileUploadValidator
{
    // Extensiones permitidas por tipo de contenido
    private static readonly Dictionary<string, string[]> AllowedExtensions = new()
    {
        { "image", new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" } },
        { "document", new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" } },
        { "video", new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" } },
        { "all", new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".mp4" } }
    };

    // Tamaños máximos por tipo (en bytes)
    private static readonly Dictionary<string, long> MaxFileSizes = new()
    {
        { "image", 5 * 1024 * 1024 },      // 5 MB
        { "document", 10 * 1024 * 1024 },  // 10 MB
        { "video", 50 * 1024 * 1024 },     // 50 MB
        { "default", 10 * 1024 * 1024 }    // 10 MB default
    };

    // Firmas de archivos (magic numbers) para validar contenido real
    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        {
            ".jpg", new[]
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
            }
        },
        {
            ".png", new[]
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        },
        {
            ".pdf", new[]
            {
                new byte[] { 0x25, 0x50, 0x44, 0x46 }
            }
        },
        {
            ".docx", new[]
            {
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            }
        },
        {
            ".mp4", new[]
            {
                new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 },
                new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 }
            }
        }
    };

    /// <summary>
    /// Valida un archivo completo
    /// </summary>
    public static async Task<ValidationResult> ValidateAsync(IBrowserFile file, string fileType = "all", CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult();

        // 1. Validar nombre de archivo
        if (string.IsNullOrWhiteSpace(file.Name))
        {
            result.AddError("El nombre del archivo no puede estar vacío");
            return result;
        }

        // 2. Validar caracteres peligrosos en nombre
        if (ContainsDangerousCharacters(file.Name))
        {
            result.AddError("El nombre del archivo contiene caracteres no permitidos");
            return result;
        }

        // 3. Validar extensión
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();
        if (!AllowedExtensions[fileType].Contains(extension))
        {
            result.AddError($"Tipo de archivo no permitido. Permitidos: {string.Join(", ", AllowedExtensions[fileType])}");
            return result;
        }

        // 4. Validar tamaño
        var maxSize = MaxFileSizes.ContainsKey(fileType) ? MaxFileSizes[fileType] : MaxFileSizes["default"];
        if (file.Size > maxSize)
        {
            result.AddError($"El archivo es demasiado grande. Tamaño máximo: {FormatBytes(maxSize)}");
            return result;
        }

        if (file.Size == 0)
        {
            result.AddError("El archivo está vacío");
            return result;
        }

        // 5. Validar firma del archivo (magic numbers) - DESACTIVADO para sistema cerrado
        // if (FileSignatures.ContainsKey(extension))
        // {
        //     var isValid = await ValidateFileSignatureAsync(file, extension, cancellationToken);
        //     if (!isValid)
        //     {
        //         result.AddError("El contenido del archivo no coincide con su extensión. Posible archivo malicioso.");
        //         return result;
        //     }
        // }

        // 6. Escaneo básico de contenido malicioso
        var isMalicious = await ScanForMaliciousContentAsync(file, cancellationToken);
        if (isMalicious)
        {
            result.AddError("El archivo contiene contenido potencialmente malicioso");
            return result;
        }

        result.IsValid = true;
        return result;
    }

    /// <summary>
    /// Valida múltiples archivos
    /// </summary>
    public static async Task<MultiFileValidationResult> ValidateMultipleAsync(IReadOnlyList<IBrowserFile> files, string fileType = "all", int maxFiles = 10, CancellationToken cancellationToken = default)
    {
        var multiResult = new MultiFileValidationResult();

        if (files.Count > maxFiles)
        {
            multiResult.AddError($"Demasiados archivos. Máximo permitido: {maxFiles}");
            return multiResult;
        }

        foreach (var file in files)
        {
            var result = await ValidateAsync(file, fileType, cancellationToken);
            multiResult.FileResults.Add(file.Name, result);
            
            if (!result.IsValid)
            {
                multiResult.IsValid = false;
            }
        }

        if (multiResult.FileResults.All(x => x.Value.IsValid))
        {
            multiResult.IsValid = true;
        }

        return multiResult;
    }

    /// <summary>
    /// Genera nombre de archivo seguro
    /// </summary>
    public static string GenerateSafeFileName(string originalName)
    {
        var extension = Path.GetExtension(originalName);
        var safeExtension = SanitizeFileName(extension);
        var uniqueId = Guid.NewGuid().ToString("N");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        
        return $"{timestamp}_{uniqueId}{safeExtension}";
    }

    /// <summary>
    /// Sanitiza nombre de archivo removiendo caracteres peligrosos
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        // Remover caracteres peligrosos
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // Remover caracteres especiales adicionales
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^\w\.-]", "_");
        
        // Limitar longitud
        if (sanitized.Length > 100)
        {
            var extension = Path.GetExtension(sanitized);
            sanitized = sanitized.Substring(0, 100 - extension.Length) + extension;
        }

        return sanitized;
    }

    private static bool ContainsDangerousCharacters(string fileName)
    {
        // Patrones peligrosos
        var dangerousPatterns = new[]
        {
            "..",           // Directory traversal
            "\\",           // Path separator
            "/",            // Path separator
            ":",            // Drive letter
            "<",            // Redirection
            ">",            // Redirection
            "|",            // Pipe
            "\"",           // Quote
            "\0",           // Null byte
            "\n",           // Newline
            "\r"            // Carriage return
        };

        return dangerousPatterns.Any(fileName.Contains);
    }

    private static async Task<bool> ValidateFileSignatureAsync(IBrowserFile file, string extension, CancellationToken cancellationToken)
    {
        var signatures = FileSignatures[extension];
        var headerBytes = new byte[8]; // Leer primeros 8 bytes

        try
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 1024); // Solo leer header
            await stream.ReadAsync(headerBytes, 0, headerBytes.Length, cancellationToken);

            return signatures.Any(signature => 
                headerBytes.Take(signature.Length).SequenceEqual(signature));
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> ScanForMaliciousContentAsync(IBrowserFile file, CancellationToken cancellationToken)
    {
        // Lista de patrones maliciosos conocidos
        var maliciousPatterns = new[]
        {
            "<script",
            "javascript:",
            "onerror=",
            "onclick=",
            "onload=",
            "eval(",
            "base64,",
            "data:text/html"
        };

        try
        {
            // Para archivos de texto, escanear contenido
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (extension == ".txt" || extension == ".html" || extension == ".xml")
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024); // Max 1MB para escaneo
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                
                return maliciousPatterns.Any(pattern => 
                    content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
            }

            return false; // No malicioso por defecto
        }
        catch
        {
            return true; // Asumir malicioso si falla el escaneo
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// Resultado de validación de un archivo
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> Errors { get; set; } = new();

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public string GetErrorMessage()
    {
        return string.Join("; ", Errors);
    }
}

/// <summary>
/// Resultado de validación de múltiples archivos
/// </summary>
public class MultiFileValidationResult
{
    public bool IsValid { get; set; } = false;
    public Dictionary<string, ValidationResult> FileResults { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public string GetErrorMessage()
    {
        var allErrors = Errors.ToList();
        foreach (var fileResult in FileResults.Where(x => !x.Value.IsValid))
        {
            allErrors.Add($"{fileResult.Key}: {fileResult.Value.GetErrorMessage()}");
        }
        return string.Join("\n", allErrors);
    }
}
