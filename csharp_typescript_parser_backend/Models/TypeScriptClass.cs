namespace TypeScriptParserBackend.Models;

/// <summary>
/// Represents a TypeScript class, its heritage, fields, and methods.
/// </summary>
public class TypeScriptClass
{
    /// <summary>
    /// Class name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Heritage (extends/implements).
    /// </summary>
    public HeritageClause Heritage { get; set; } = new();

    /// <summary>
    /// Class fields.
    /// </summary>
    public List<VariableDeclaration> Fields { get; set; } = new();

    /// <summary>
    /// Class methods.
    /// </summary>
    public List<FunctionDeclaration> Methods { get; set; } = new();
}
