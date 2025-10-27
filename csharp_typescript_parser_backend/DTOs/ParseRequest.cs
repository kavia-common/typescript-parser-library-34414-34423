using System.ComponentModel.DataAnnotations;

namespace TypeScriptParserBackend.DTOs;

/// <summary>
/// Request DTO for parse/variables endpoints.
/// </summary>
public class ParseRequest
{
    /// <summary>
    /// TypeScript source text.
    /// </summary>
    [Required]
    public string Source { get; set; } = string.Empty;
}
