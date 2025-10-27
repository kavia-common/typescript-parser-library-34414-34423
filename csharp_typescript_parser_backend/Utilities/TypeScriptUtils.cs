using System.Text.RegularExpressions;

namespace TypeScriptParserBackend.Utilities;

/// <summary>
/// Utility helpers for TypeScript parsing.
/// </summary>
public static class TypeScriptUtils
{
    private static readonly Regex SpaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static string NormalizeSpaces(string input)
    {
        return SpaceRegex.Replace(input, " ").Trim();
    }

    public static bool IsIdentifierStart(char c) =>
        char.IsLetter(c) || c == '_' || c == '$';

    public static bool IsIdentifierPart(char c) =>
        char.IsLetterOrDigit(c) || c == '_' || c == '$';
}
