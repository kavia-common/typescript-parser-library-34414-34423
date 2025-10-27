using TypeScriptParserBackend.Models;

namespace TypeScriptParserBackend.DTOs;

/// <summary>
/// Response DTO for parse endpoint containing structured AST-like info.
/// </summary>
public class ParseResponse
{
    /// <summary>
    /// Collected import statements.
    /// </summary>
    public List<ImportStatement> Imports { get; set; } = new();

    /// <summary>
    /// Parsed classes (at most one primary class for this simplified parser).
    /// </summary>
    public List<TypeScriptClass> Classes { get; set; } = new();

    /// <summary>
    /// Top-level functions (not methods).
    /// </summary>
    public List<FunctionDeclaration> Functions { get; set; } = new();

    /// <summary>
    /// Diagnostics and warnings collected during parsing.
    /// </summary>
    public List<string> Diagnostics { get; set; } = new();
}
