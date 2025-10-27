using System.Text;

namespace TypeScriptParserBackend.Utilities;

/// <summary>
/// A small helper to generate formatted code with indentation.
/// </summary>
public class CodeStringBuilder
{
    private readonly StringBuilder _sb = new();
    private int _indentLevel;
    private readonly string _indentUnit;

    public CodeStringBuilder(string indentUnit = "    ")
    {
        _indentUnit = indentUnit;
    }

    public CodeStringBuilder Indent() { _indentLevel++; return this; }
    public CodeStringBuilder Outdent() { if (_indentLevel > 0) _indentLevel--; return this; }

    public CodeStringBuilder Append(string text)
    {
        _sb.Append(text);
        return this;
    }

    public CodeStringBuilder AppendLine(string text = "")
    {
        if (text.Length > 0)
        {
            _sb.Append(string.Concat(Enumerable.Repeat(_indentUnit, _indentLevel)));
            _sb.Append(text);
        }
        _sb.AppendLine();
        return this;
    }

    public override string ToString() => _sb.ToString();
}
