namespace TypeScriptParserBackend.Models;

/// <summary>
/// Represents a function or method declaration (simplified).
/// </summary>
public class FunctionDeclaration
{
    /// <summary>
    /// Function or method name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parameter list as raw text inside parentheses.
    /// </summary>
    public string Parameters { get; set; } = string.Empty;

    /// <summary>
    /// Return type annotation if any.
    /// </summary>
    public string? ReturnType { get; set; }

    /// <summary>
    /// True if this is a class method.
    /// </summary>
    public bool IsMethod { get; set; }

    /// <summary>
    /// Access modifier for class methods.
    /// </summary>
    public string? Modifier { get; set; }

    /// <summary>
    /// Raw body string between braces (not parsed).
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
