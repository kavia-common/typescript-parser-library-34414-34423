namespace TypeScriptParserBackend.Errors;

/// <summary>
/// Represents a diagnostic error or warning.
/// </summary>
public class ErrorInfo
{
    public string Message { get; set; } = string.Empty;
    public int? Line { get; set; }
    public int? Column { get; set; }

    public override string ToString()
    {
        return Line.HasValue
            ? $"{Message} (line {Line}{(Column.HasValue ? $", col {Column}" : "")})"
            : Message;
    }
}
