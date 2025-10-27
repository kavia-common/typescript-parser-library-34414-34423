namespace TypeScriptParserBackend.Parsing;

/// <summary>
/// Lightweight tokenizer splitting by semicolons and braces while being aware of strings,
/// template literals and single/multi-line comments. Produces logical segments to help parsing.
/// </summary>
public static class Tokenizer
{
    public static List<string> SplitTopLevelSegments(string source)
    {
        var segments = new List<string>();
        var current = new System.Text.StringBuilder();

        bool inSingle = false, inDouble = false, inTemplate = false;
        bool inLineComment = false, inBlockComment = false;

        int braceDepth = 0;
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            char? n = i + 1 < source.Length ? source[i + 1] : (char?)null;

            // handle comment transitions
            if (!inSingle && !inDouble && !inTemplate && !inBlockComment && !inLineComment)
            {
                if (c == '/' && n == '/')
                {
                    inLineComment = true;
                    current.Append(c);
                    continue;
                }
                if (c == '/' && n == '*')
                {
                    inBlockComment = true;
                    current.Append(c);
                    continue;
                }
            }
            else
            {
                if (inLineComment)
                {
                    current.Append(c);
                    if (c == '\n')
                    {
                        inLineComment = false;
                    }
                    continue;
                }
                if (inBlockComment)
                {
                    current.Append(c);
                    if (c == '*' && n == '/')
                    {
                        current.Append(n);
                        i++;
                        inBlockComment = false;
                    }
                    continue;
                }
            }

            // string literal state machine
            if (!inTemplate && !inSingle && !inDouble)
            {
                if (c == '\'') { inSingle = true; current.Append(c); continue; }
                if (c == '\"') { inDouble = true; current.Append(c); continue; }
                if (c == '`') { inTemplate = true; current.Append(c); continue; }
            }
            else
            {
                current.Append(c);

                if (c == '\\') // escape next char
                {
                    if (n.HasValue) { current.Append(n.Value); i++; }
                    continue;
                }

                if (inSingle && c == '\'') inSingle = false;
                else if (inDouble && c == '\"') inDouble = false;
                else if (inTemplate && c == '`') inTemplate = false;

                continue;
            }

            // track braces for top-level segmentation
            if (c == '{') braceDepth++;
            if (c == '}') braceDepth = Math.Max(0, braceDepth - 1);

            current.Append(c);

            // split at top-level semicolons or class/function blocks completion
            if (braceDepth == 0 && (c == ';' || c == '}'))
            {
                var seg = current.ToString().Trim();
                if (seg.Length > 0) segments.Add(seg);
                current.Clear();
            }
        }

        var last = current.ToString().Trim();
        if (last.Length > 0) segments.Add(last);

        return segments;
    }
}
