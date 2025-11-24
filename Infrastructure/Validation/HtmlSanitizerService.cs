using Ganss.Xss;

namespace Sistema_Experto_ONG_Juventud_Sin_Limites.Infrastructure.Validation;

/// <summary>
/// Servicio de sanitización HTML para prevenir ataques XSS
/// </summary>
public class HtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;
    private readonly HtmlSanitizer _strictSanitizer;

    public HtmlSanitizerService()
    {
        // Sanitizador estándar (permite algunos tags seguros)
        _sanitizer = new HtmlSanitizer();
        ConfigureStandardSanitizer();

        // Sanitizador estricto (solo texto)
        _strictSanitizer = new HtmlSanitizer();
        ConfigureStrictSanitizer();
    }

    /// <summary>
    /// Sanitiza HTML permitiendo algunos tags seguros (para contenido rich text)
    /// </summary>
    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _sanitizer.Sanitize(html);
    }

    /// <summary>
    /// Sanitiza HTML de forma estricta (solo texto plano)
    /// </summary>
    public string SanitizeStrict(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _strictSanitizer.Sanitize(html);
    }

    /// <summary>
    /// Sanitiza lista de strings
    /// </summary>
    public List<string> SanitizeList(IEnumerable<string>? items, bool strict = false)
    {
        if (items == null)
            return new List<string>();

        return items
            .Select(item => strict ? SanitizeStrict(item) : Sanitize(item))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    /// <summary>
    /// Valida si un string contiene HTML potencialmente peligroso
    /// </summary>
    public bool ContainsDangerousHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var sanitized = SanitizeStrict(input);
        return sanitized != input;
    }

    /// <summary>
    /// Extrae texto plano de HTML
    /// </summary>
    public string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _strictSanitizer.Sanitize(html);
    }

    private void ConfigureStandardSanitizer()
    {
        // Tags permitidos para contenido rich text
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("br");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("b");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("i");
        _sanitizer.AllowedTags.Add("u");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");
        _sanitizer.AllowedTags.Add("h1");
        _sanitizer.AllowedTags.Add("h2");
        _sanitizer.AllowedTags.Add("h3");
        _sanitizer.AllowedTags.Add("h4");
        _sanitizer.AllowedTags.Add("h5");
        _sanitizer.AllowedTags.Add("h6");
        _sanitizer.AllowedTags.Add("blockquote");
        _sanitizer.AllowedTags.Add("code");
        _sanitizer.AllowedTags.Add("pre");
        _sanitizer.AllowedTags.Add("a");

        // Atributos permitidos
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("title");
        _sanitizer.AllowedAttributes.Add("target");

        // Esquemas URI permitidos (solo http y https)
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");

        // CSS properties permitidas (ninguna por defecto)
        _sanitizer.AllowedCssProperties.Clear();

        // Remover tags no permitidos completamente (incluyendo su contenido para tags peligrosos)
        _sanitizer.KeepChildNodes = false;
    }

    private void ConfigureStrictSanitizer()
    {
        // No permitir ningún tag HTML
        _strictSanitizer.AllowedTags.Clear();
        _strictSanitizer.AllowedAttributes.Clear();
        _strictSanitizer.AllowedSchemes.Clear();
        _strictSanitizer.AllowedCssProperties.Clear();
        _strictSanitizer.KeepChildNodes = true; // Remover completamente tags peligrosos con su contenido
    }
}

/// <summary>
/// Extension methods para sanitización fácil
/// </summary>
public static class HtmlSanitizerExtensions
{
    private static readonly HtmlSanitizerService _service = new();

    /// <summary>
    /// Sanitiza string removiendo HTML peligroso
    /// </summary>
    public static string SanitizeHtml(this string? input)
    {
        return _service.Sanitize(input);
    }

    /// <summary>
    /// Sanitiza string removiendo TODO el HTML
    /// </summary>
    public static string SanitizeHtmlStrict(this string? input)
    {
        return _service.SanitizeStrict(input);
    }

    /// <summary>
    /// Valida si contiene HTML peligroso
    /// </summary>
    public static bool HasDangerousHtml(this string? input)
    {
        return _service.ContainsDangerousHtml(input);
    }
}
