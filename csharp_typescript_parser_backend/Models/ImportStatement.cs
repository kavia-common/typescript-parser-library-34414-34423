using System.Text.Json.Serialization;

namespace TypeScriptParserBackend.Models;

/// <summary>
/// Represents a TypeScript import statement.
/// </summary>
public class ImportStatement
{
    /// <summary>
    /// The raw import line.
    /// </summary>
    public string Raw { get; set; } = string.Empty;

    /// <summary>
    /// The module specifier (from 'module').
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Default import if present.
    /// </summary>
    public string? DefaultImport { get; set; }

    /// <summary>
    /// Named imports list.
    /// </summary>
    public List<string> NamedImports { get; set; } = new();

    /// <summary>
    /// Namespace import like * as X
    /// </summary>
    public string? NamespaceAlias { get; set; }

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(NamespaceAlias))
        {
            return $"import * as {NamespaceAlias} from \"{Module}\";";
        }

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(DefaultImport))
        {
            parts.Add(DefaultImport!);
        }
        if (NamedImports.Count > 0)
        {
            parts.Add("{" + string.Join(", ", NamedImports) + "}");
        }

        if (parts.Count == 0)
        {
            return $"import \"{Module}\";";
        }

        return $"import {string.Join(", ", parts)} from \"{Module}\";";
    }
}
