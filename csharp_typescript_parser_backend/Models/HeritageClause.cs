namespace TypeScriptParserBackend.Models;

/// <summary>
/// Represents TypeScript class heritage information (extends, implements).
/// </summary>
public class HeritageClause
{
    /// <summary>
    /// The single base class (extends) if any.
    /// </summary>
    public string? BaseClass { get; set; }

    /// <summary>
    /// The list of interfaces implemented by the class.
    /// </summary>
    public List<string> Implements { get; set; } = new();
}
