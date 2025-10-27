namespace TypeScriptParserBackend.Models;

/// <summary>
/// Represents a variable/property declaration.
/// </summary>
public class VariableDeclaration
{
    /// <summary>
    /// Variable or property name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type annotation if present.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Initial value (raw text) if present.
    /// </summary>
    public string? Initializer { get; set; }

    /// <summary>
    /// Access modifier (public|private|protected|readonly etc.).
    /// </summary>
    public string? Modifier { get; set; }

    /// <summary>
    /// True if it is a class field; false if top-level variable.
    /// </summary>
    public bool IsClassField { get; set; }
}
