using System.Text.RegularExpressions;
using TypeScriptParserBackend.DTOs;
using TypeScriptParserBackend.Errors;
using TypeScriptParserBackend.Models;
using TypeScriptParserBackend.Utilities;

namespace TypeScriptParserBackend.Parsing;

/// <summary>
/// Minimal TypeScript parser focused on imports, classes (name/heritage/fields/methods) and top-level functions.
/// Not a full TS parser; uses regex and token segments to extract primary structures.
/// </summary>
public class TypeScriptParser
{
    private static readonly Regex ImportRegex = new(
        @"^import\s+(?:(?<default>[A-Za-z0-9_$]+)\s*,\s*)?(?:(\*\s+as\s+(?<ns>[A-Za-z0-9_$]+))|(\{(?<named>[^\}]*)\}))?\s*from\s*[\""\'](?<module>[^\""\']+)[\""\']\s*;?$|^import\s*[\""\'](?<onlymodule>[^\""\']+)[\""\']\s*;?$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex ClassHeaderRegex = new(
        @"class\s+(?<name>[A-Za-z0-9_$]+)(?:\s+extends\s+(?<extends>[A-Za-z0-9_$.]+))?(?:\s+implements\s+(?<implements>[^{]+))?",
        RegexOptions.Compiled);

    private static readonly Regex FunctionRegex = new(
        @"^function\s+(?<name>[A-Za-z0-9_$]+)\s*\((?<params>[^\)]*)\)\s*(?::\s*(?<ret>[^ \{]+))?\s*\{(?<body>[\s\S]*)\}\s*$",
        RegexOptions.Compiled);

    private static readonly Regex MethodRegex = new(
        @"^(?<mod>(?:public|private|protected|readonly|static)\s+)*?(?<name>[A-Za-z0-9_$]+)\s*\((?<params>[^\)]*)\)\s*(?::\s*(?<ret>[^ \{]+))?\s*\{(?<body>[\s\S]*)\}\s*$",
        RegexOptions.Compiled);

    private static readonly Regex FieldRegex = new(
        @"^(?<mod>(?:public|private|protected|readonly|static)\s+)*?(?<name>[A-Za-z0-9_$]+)\s*(?::\s*(?<type>[^=;]+))?\s*(?:=\s*(?<init>[^;]+))?;?$",
        RegexOptions.Compiled);

    /// <summary>
    /// Parse the given TypeScript source into a structured response.
    /// </summary>
    // PUBLIC_INTERFACE
    public ParseResponse Parse(string source)
    {
        var resp = new ParseResponse();
        try
        {
            var segments = Tokenizer.SplitTopLevelSegments(source);

            // imports
            foreach (var seg in segments.Where(s => s.TrimStart().StartsWith("import ")))
            {
                var m = ImportRegex.Match(seg.Trim());
                if (m.Success)
                {
                    var stmt = new ImportStatement
                    {
                        Raw = seg.Trim(),
                        Module = m.Groups["module"].Success ? m.Groups["module"].Value :
                                 (m.Groups["onlymodule"].Success ? m.Groups["onlymodule"].Value : string.Empty),
                        DefaultImport = m.Groups["default"].Success ? m.Groups["default"].Value : null,
                        NamespaceAlias = m.Groups["ns"].Success ? m.Groups["ns"].Value : null
                    };
                    if (m.Groups["named"].Success)
                    {
                        var inner = m.Groups["named"].Value;
                        var names = inner.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
                        stmt.NamedImports.AddRange(names);
                    }
                    resp.Imports.Add(stmt);
                }
                else
                {
                    resp.Diagnostics.Add($"Unrecognized import format: {seg.Trim()}");
                }
            }

            // class and functions detection: join content to catch class blocks
            var classMatch = Regex.Match(source, @"class\s+[A-Za-z0-9_$]+\s*[\s\S]*?\{[\s\S]*\}", RegexOptions.Multiline);
            if (classMatch.Success)
            {
                var headerMatch = ClassHeaderRegex.Match(classMatch.Value);
                if (headerMatch.Success)
                {
                    var cls = new TypeScriptClass
                    {
                        Name = headerMatch.Groups["name"].Value
                    };
                    var ext = headerMatch.Groups["extends"]?.Value;
                    if (!string.IsNullOrWhiteSpace(ext))
                    {
                        cls.Heritage.BaseClass = ext.Trim();
                    }
                    var impl = headerMatch.Groups["implements"]?.Value;
                    if (!string.IsNullOrWhiteSpace(impl))
                    {
                        var interfaces = impl.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);
                        cls.Heritage.Implements.AddRange(interfaces);
                    }

                    // Extract body between first { after 'class ...' and its matching }
                    var classStart = classMatch.Value.IndexOf("{", headerMatch.Index + headerMatch.Length, StringComparison.Ordinal);
                    if (classStart < 0) classStart = classMatch.Value.IndexOf('{');
                    var body = ExtractBraceBody(classMatch.Value, classMatch.Value.IndexOf('{'));

                    // parse class members by naive split; then match field or method regex
                    var memberSegments = SplitMembers(body);
                    foreach (var mem in memberSegments)
                    {
                        var trimmed = mem.Trim();
                        if (trimmed.Length == 0) continue;

                        var mm = MethodRegex.Match(trimmed);
                        if (mm.Success)
                        {
                            cls.Methods.Add(new FunctionDeclaration
                            {
                                Name = mm.Groups["name"].Value,
                                Parameters = mm.Groups["params"].Value.Trim(),
                                ReturnType = mm.Groups["ret"].Success ? mm.Groups["ret"].Value.Trim() : null,
                                Body = ExtractBraceBody(trimmed, trimmed.IndexOf('{')),
                                IsMethod = true,
                                Modifier = mm.Groups["mod"].Success ? TypeScriptUtils.NormalizeSpaces(mm.Groups["mod"].Value ?? string.Empty) : null
                            });
                            continue;
                        }

                        var fm = FieldRegex.Match(trimmed);
                        if (fm.Success)
                        {
                            cls.Fields.Add(new VariableDeclaration
                            {
                                Name = fm.Groups["name"].Value.Trim(),
                                Type = fm.Groups["type"].Success ? fm.Groups["type"].Value.Trim() : null,
                                Initializer = fm.Groups["init"].Success ? fm.Groups["init"].Value.Trim() : null,
                                Modifier = fm.Groups["mod"].Success ? TypeScriptUtils.NormalizeSpaces(fm.Groups["mod"].Value ?? string.Empty) : null,
                                IsClassField = true
                            });
                            continue;
                        }

                        // unknown member
                        resp.Diagnostics.Add($"Unrecognized class member: {trimmed}");
                    }

                    resp.Classes.Add(cls);
                }
                else
                {
                    resp.Diagnostics.Add("Could not parse class header.");
                }
            }

            // top-level functions (non-methods)
            foreach (var seg in segments)
            {
                var s = seg.Trim();
                if (s.StartsWith("function "))
                {
                    var fm = FunctionRegex.Match(NormalizeBraces(s));
                    if (fm.Success)
                    {
                        resp.Functions.Add(new FunctionDeclaration
                        {
                            Name = fm.Groups["name"].Value,
                            Parameters = fm.Groups["params"].Value.Trim(),
                            ReturnType = fm.Groups["ret"].Success ? fm.Groups["ret"].Value.Trim() : null,
                            Body = fm.Groups["body"].Value,
                            IsMethod = false
                        });
                    }
                    else
                    {
                        resp.Diagnostics.Add($"Unrecognized function format: {s}");
                    }
                }
            }
        }
        catch (ParseException ex)
        {
            resp.Diagnostics.Add($"Parse error: {ex.Message} {(ex.Line.HasValue ? $"(line {ex.Line})" : "")}");
        }
        catch (Exception ex)
        {
            resp.Diagnostics.Add($"Unhandled parse error: {ex.Message}");
        }

        return resp;
    }

    /// <summary>
    /// Collects variables from the parsed result (class fields).
    /// </summary>
    // PUBLIC_INTERFACE
    public List<VariableDeclaration> Variables(ParseResponse parseResult)
    {
        var vars = new List<VariableDeclaration>();
        foreach (var c in parseResult.Classes)
        {
            vars.AddRange(c.Fields);
        }
        return vars;
    }

    /// <summary>
    /// Generates TypeScript source code from the parsed result, optionally targeting a given class name.
    /// </summary>
    // PUBLIC_INTERFACE
    public string ToString(ParseResponse parseResult, string? className = null)
    {
        var b = new CodeStringBuilder();

        // imports
        foreach (var imp in parseResult.Imports)
        {
            b.AppendLine(imp.ToString());
        }
        if (parseResult.Imports.Count > 0) b.AppendLine();

        // choose class
        var cls = parseResult.Classes.FirstOrDefault(c => className == null || c.Name == className);
        if (cls != null)
        {
            var header = $"class {cls.Name}";
            if (!string.IsNullOrWhiteSpace(cls.Heritage.BaseClass))
            {
                header += $" extends {cls.Heritage.BaseClass}";
            }
            if (cls.Heritage.Implements.Count > 0)
            {
                header += $" implements {string.Join(", ", cls.Heritage.Implements)}";
            }

            b.AppendLine($"{header} {{");
            b.Indent();

            // fields
            foreach (var f in cls.Fields)
            {
                var line = string.Empty;
                if (!string.IsNullOrWhiteSpace(f.Modifier))
                {
                    line += f.Modifier + " ";
                }
                line += f.Name;
                if (!string.IsNullOrWhiteSpace(f.Type))
                {
                    line += $": {f.Type}";
                }
                if (!string.IsNullOrWhiteSpace(f.Initializer))
                {
                    line += $" = {f.Initializer}";
                }
                line += ";";
                b.AppendLine(line);
            }

            if (cls.Fields.Count > 0 && cls.Methods.Count > 0)
            {
                b.AppendLine();
            }

            // methods
            foreach (var m in cls.Methods)
            {
                var headerLine = string.Empty;
                if (!string.IsNullOrWhiteSpace(m.Modifier))
                {
                    headerLine += m.Modifier + " ";
                }
                headerLine += $"{m.Name}({m.Parameters})";
                if (!string.IsNullOrWhiteSpace(m.ReturnType))
                {
                    headerLine += $": {m.ReturnType}";
                }
                headerLine += " {";
                b.AppendLine(headerLine);
                b.Indent();
                foreach (var line in m.Body.Split('\n'))
                {
                    var l = line.TrimEnd('\r');
                    b.AppendLine(l);
                }
                b.Outdent();
                b.AppendLine("}");
                b.AppendLine();
            }

            b.Outdent();
            b.AppendLine("}");
            b.AppendLine();
        }

        // top-level functions
        foreach (var f in parseResult.Functions)
        {
            var line = $"function {f.Name}({f.Parameters})";
            if (!string.IsNullOrWhiteSpace(f.ReturnType))
            {
                line += $": {f.ReturnType}";
            }
            line += " {";
            b.AppendLine(line);
            b.Indent();
            foreach (var l in f.Body.Split('\n'))
            {
                b.AppendLine(l.TrimEnd('\r'));
            }
            b.Outdent();
            b.AppendLine("}");
            b.AppendLine();
        }

        return b.ToString().TrimEnd();
    }

    private static string ExtractBraceBody(string text, int braceIndex)
    {
        if (braceIndex < 0 || braceIndex >= text.Length || text[braceIndex] != '{')
        {
            return string.Empty;
        }
        int depth = 0;
        for (int i = braceIndex; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    // body without outer braces
                    return text.Substring(braceIndex + 1, i - braceIndex - 1);
                }
            }
        }
        return string.Empty;
    }

    private static string NormalizeBraces(string s)
    {
        // ensure braces for regex that expects function {...}
        if (!s.Contains("{"))
        {
            return s + " {}";
        }
        return s;
    }

    private static List<string> SplitMembers(string classBody)
    {
        // split class members by top-level semicolons and method blocks
        var result = new List<string>();
        var cur = new System.Text.StringBuilder();

        bool inSingle = false, inDouble = false, inTemplate = false;
        int depth = 0;

        for (int i = 0; i < classBody.Length; i++)
        {
            var c = classBody[i];
            var n = i + 1 < classBody.Length ? classBody[i + 1] : (char?)null;

            if (!inSingle && !inDouble && !inTemplate)
            {
                if (c == '\'') { inSingle = true; cur.Append(c); continue; }
                if (c == '\"') { inDouble = true; cur.Append(c); continue; }
                if (c == '`') { inTemplate = true; cur.Append(c); continue; }
            }
            else
            {
                cur.Append(c);
                if (c == '\\')
                {
                    if (n.HasValue) { cur.Append(n.Value); i++; }
                    continue;
                }
                if (inSingle && c == '\'') inSingle = false;
                else if (inDouble && c == '\"') inDouble = false;
                else if (inTemplate && c == '`') inTemplate = false;
                continue;
            }

            if (c == '{') depth++;
            if (c == '}') depth = Math.Max(0, depth - 1);

            cur.Append(c);

            if (depth == 0 && (c == ';' || (c == '}' && cur.ToString().Contains("{"))))
            {
                var seg = cur.ToString().Trim();
                if (seg.Length > 0) result.Add(seg);
                cur.Clear();
            }
        }

        var last = cur.ToString().Trim();
        if (last.Length > 0) result.Add(last);

        return result;
    }
}
