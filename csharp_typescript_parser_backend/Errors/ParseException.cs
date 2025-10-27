namespace TypeScriptParserBackend.Errors;

/// <summary>
/// Exception thrown when parsing fails.
/// </summary>
public class ParseException : Exception
{
    public int? Line { get; }
    public int? Column { get; }

    public ParseException(string message, int? line = null, int? column = null, Exception? inner = null)
        : base(message, inner)
    {
        Line = line;
        Column = column;
    }
}
