using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TypescriptParser.Models;

namespace TypescriptParser.Parser
{
    /// <summary>
    /// A lightweight TypeScript class file parser using regex and balanced brace scanning.
    /// It is not a full grammar parser; it aims to be resilient for common patterns.
    /// </summary>
    internal sealed class TypescriptClassParser
    {
        // Regex to match different import forms
        private static readonly Regex ImportRegex = new Regex(
            @"^\s*import\s+(type\s+)?(?:(?<default>[A-Za-z0-9_\$]+)\s*,\s*)?(?:(\*\s+as\s+(?<ns>[A-Za-z0-9_\$]+))|(\{\s*(?<named>[^\}]+)\s*\}))?\s*from\s*['""](?<from>[^'""]+)['""]\s*;|^\s*import\s+(type\s+)?['""](?<fromBare>[^'""]+)['""]\s*;",
            RegexOptions.Compiled | RegexOptions.Multiline);

        // Regex to find the class declaration signature
        private static readonly Regex ClassSignatureRegex = new Regex(
            @"class\s+(?<name>[A-Za-z0-9_\$]+)\s*(?:extends\s+(?<base>[A-Za-z0-9_\$\.]+))?\s*(?:implements\s+(?<impl>[\w\.,\s]+))?\s*\{",
            RegexOptions.Compiled);

        // Regex to capture class fields/properties lines (simplified)
        private static readonly Regex FieldRegex = new Regex(
            @"^(?<indent>\s*)(?<access>public|private|protected)?\s*(?<static>static\s+)?(?<readonly>readonly\s+)?(?<name>[A-Za-z0-9_\$]+)(?<opt>\?)?\s*(?::\s*(?<type>[^=;]+))?\s*(?:=\s*(?<init>[^;]+))?;",
            RegexOptions.Compiled | RegexOptions.Multiline);

        // Regex to capture method signatures (without body) from a block of text
        private static readonly Regex MethodSignatureRegex = new Regex(
            @"^(?<indent>\s*)(?<access>public|private|protected)?\s*(?<static>static\s+)?(?<async>async\s+)?(?<name>[A-Za-z0-9_\$]+)\s*\((?<params>[^\)]*)\)\s*(?::\s*(?<ret>[^ \{]+))?\s*\{",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public ClassDefinition Parse(string source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var model = new ClassDefinition
            {
                RawSource = source
            };

            // 1. Imports
            foreach (Match m in ImportRegex.Matches(source))
            {
                var isTypeOnly = !string.IsNullOrEmpty(m.Groups[1].Value);

                var fromBare = m.Groups["fromBare"]?.Value;
                var from = string.IsNullOrWhiteSpace(fromBare)
                    ? m.Groups["from"]?.Value
                    : fromBare;

                var decl = new ImportDeclaration
                {
                    IsTypeOnly = isTypeOnly,
                    FromModule = from ?? string.Empty,
                    Raw = m.Value.Trim()
                };

                var d = m.Groups["default"]?.Value;
                if (!string.IsNullOrWhiteSpace(d))
                    decl.DefaultImport = d.Trim();

                var ns = m.Groups["ns"]?.Value;
                if (!string.IsNullOrWhiteSpace(ns))
                    decl.NamespaceImport = ns.Trim();

                var named = m.Groups["named"]?.Value;
                if (!string.IsNullOrWhiteSpace(named))
                {
                    // split by comma, keep "as" aliases intact
                    foreach (var part in named.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var clean = part.Trim();
                        if (!string.IsNullOrWhiteSpace(clean))
                            decl.NamedImports.Add(clean);
                    }
                }

                model.Imports.Add(decl);
            }

            // 2. Class signature and body block by scanning for balanced braces after the signature.
            var classMatch = ClassSignatureRegex.Match(source);
            if (!classMatch.Success)
            {
                // If no class found, return with imports only.
                return model;
            }

            model.ClassName = classMatch.Groups["name"].Value;
            var baseClass = classMatch.Groups["base"].Value;
            if (!string.IsNullOrWhiteSpace(baseClass))
                model.BaseClass = baseClass.Trim();

            var impl = classMatch.Groups["impl"].Value;
            if (!string.IsNullOrWhiteSpace(impl))
            {
                foreach (var i in impl.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var name = i.Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                        model.ImplementedInterfaces.Add(name);
                }
            }

            // Find the class body boundaries by brace depth
            var bodyStart = classMatch.Index + classMatch.Length - 1; // position at '{'
            var (body, _) = ExtractBalancedBlock(source, bodyStart);
            // body includes the leading '{' and trailing matching '}'.
            // Extract inner content between braces to scan members
            var inner = ExtractInner(body);

            // 3. Fields
            foreach (Match fm in FieldRegex.Matches(inner))
            {
                var v = new VariableDefinition
                {
                    AccessModifier = groupOrNull(fm, "access"),
                    IsStatic = !string.IsNullOrEmpty(groupOrNull(fm, "static")),
                    IsReadonly = !string.IsNullOrEmpty(groupOrNull(fm, "readonly")),
                    Name = groupOrNull(fm, "name") ?? string.Empty,
                    IsOptional = !string.IsNullOrEmpty(groupOrNull(fm, "opt")),
                    Type = groupOrNull(fm, "type")?.Trim(),
                    Initializer = groupOrNull(fm, "init")?.Trim(),
                    Raw = fm.Value.Trim()
                };
                model.Variables.Add(v);
            }

            // 4. Methods (scan by signature, then capture full body by depth from the signature brace)
            foreach (Match mm in MethodSignatureRegex.Matches(inner))
            {
                var methodStartInInner = mm.Index + mm.Length - 1; // at opening '{' for method
                var (methodBody, bodyLen) = ExtractBalancedBlock(inner, methodStartInInner);
                var f = new FunctionDefinition
                {
                    AccessModifier = groupOrNull(mm, "access"),
                    IsStatic = !string.IsNullOrEmpty(groupOrNull(mm, "static")),
                    IsAsync = !string.IsNullOrEmpty(groupOrNull(mm, "async")),
                    Name = groupOrNull(mm, "name") ?? string.Empty,
                    ReturnType = groupOrNull(mm, "ret")?.Trim(),
                    RawSignature = mm.Value.Trim(),
                    Body = methodBody?.Substring(0) // keep as-is
                };

                var paramStr = groupOrNull(mm, "params") ?? string.Empty;
                foreach (var p in SplitTopLevel(paramStr, ','))
                {
                    var clean = p.Trim();
                    if (!string.IsNullOrWhiteSpace(clean))
                        f.Parameters.Add(clean);
                }

                model.Functions.Add(f);
            }

            return model;
        }

        private static string? groupOrNull(Match m, string name)
        {
            return m.Groups[name]?.Success == true ? m.Groups[name]!.Value : null;
        }

        /// <summary>
        /// Extracts a balanced block starting at an opening brace '{' index in the provided text.
        /// Returns the full block (from '{' to matching '}') and the length consumed.
        /// </summary>
        private static (string block, int length) ExtractBalancedBlock(string text, int openBraceIndex)
        {
            if (openBraceIndex < 0 || openBraceIndex >= text.Length || text[openBraceIndex] != '{')
                return ("", 0);

            int depth = 0;
            int i = openBraceIndex;
            for (; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '{') depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        // include closing brace
                        var end = i;
                        var block = text.Substring(openBraceIndex, end - openBraceIndex + 1);
                        return (block, block.Length);
                    }
                }
                else if (c == '"' || c == '\'' || c == '`')
                {
                    // Skip string literals including template strings to avoid counting braces inside
                    i = ConsumeQuoted(text, i);
                }
                else if (c == '/')
                {
                    // skip comments
                    if (i + 1 < text.Length && text[i + 1] == '/')
                    {
                        i = ConsumeLineComment(text, i);
                    }
                    else if (i + 1 < text.Length && text[i + 1] == '*')
                    {
                        i = ConsumeBlockComment(text, i);
                    }
                }
            }
            return ("", 0);
        }

        /// <summary>
        /// Returns inner content without the outer pair of braces.
        /// </summary>
        private static string ExtractInner(string blockWithBraces)
        {
            if (string.IsNullOrEmpty(blockWithBraces)) return blockWithBraces;
            var start = blockWithBraces.IndexOf('{');
            var end = blockWithBraces.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                return blockWithBraces.Substring(start + 1, end - start - 1);
            }
            return blockWithBraces;
        }

        private static int ConsumeQuoted(string text, int startQuoteIdx)
        {
            char q = text[startQuoteIdx];
            int i = startQuoteIdx + 1;
            while (i < text.Length)
            {
                var c = text[i];
                if (c == '\\')
                {
                    i += 2; // skip escaped char
                    continue;
                }
                if (c == q)
                {
                    return i;
                }
                i++;
            }
            return i;
        }

        private static int ConsumeLineComment(string text, int start)
        {
            int i = start + 2; // after //
            while (i < text.Length && text[i] != '\n')
                i++;
            return i;
        }

        private static int ConsumeBlockComment(string text, int start)
        {
            int i = start + 2; // after /*
            while (i + 1 < text.Length)
            {
                if (text[i] == '*' && text[i + 1] == '/')
                {
                    return i + 1;
                }
                i++;
            }
            return i;
        }

        /// <summary>
        /// Split a string at top-level commas, ignoring commas inside angle brackets, parentheses, and square brackets.
        /// </summary>
        private static IEnumerable<string> SplitTopLevel(string input, char separator)
        {
            if (string.IsNullOrWhiteSpace(input))
                yield break;

            int depthParen = 0, depthAngle = 0, depthSquare = 0;
            int start = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                switch (c)
                {
                    case '(':
                        depthParen++;
                        break;
                    case ')':
                        depthParen = Math.Max(0, depthParen - 1);
                        break;
                    case '<':
                        depthAngle++;
                        break;
                    case '>':
                        depthAngle = Math.Max(0, depthAngle - 1);
                        break;
                    case '[':
                        depthSquare++;
                        break;
                    case ']':
                        depthSquare = Math.Max(0, depthSquare - 1);
                        break;
                    case '\'':
                    case '"':
                    case '`':
                        i = ConsumeQuoted(input, i);
                        break;
                    case '/':
                        if (i + 1 < input.Length && (input[i + 1] == '/' || input[i + 1] == '*'))
                        {
                            // comments inside params are unusual; ignore advanced handling here
                        }
                        break;
                }

                if (c == separator && depthParen == 0 && depthAngle == 0 && depthSquare == 0)
                {
                    yield return input.Substring(start, i - start);
                    start = i + 1;
                }
            }

            if (start < input.Length)
                yield return input.Substring(start);
        }
    }
}
