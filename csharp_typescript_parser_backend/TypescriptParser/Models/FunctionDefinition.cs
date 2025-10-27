using System;
using System.Collections.Generic;
using System.Text;

namespace TypescriptParser.Models
{
    /// <summary>
    /// Represents a TypeScript function or class method definition (lightweight).
    /// </summary>
    public sealed class FunctionDefinition
    {
        /// <summary>
        /// The function/method name if available. Anonymous functions may be empty.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// List of parameter strings as captured from source.
        /// </summary>
        public List<string> Parameters { get; } = new List<string>();

        /// <summary>
        /// Optional return type string.
        /// </summary>
        public string? ReturnType { get; set; }

        /// <summary>
        /// Access modifier for class methods (public/private/protected).
        /// </summary>
        public string? AccessModifier { get; set; }

        /// <summary>
        /// Whether 'static' keyword is present for class method.
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether 'async' keyword is present.
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// The raw body of the function including braces, as captured via depth scan.
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Original raw signature text.
        /// </summary>
        public string? RawSignature { get; set; }

        public override string ToString()
        {
            var mod = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(AccessModifier))
                mod.Append(AccessModifier).Append(' ');
            if (IsStatic)
                mod.Append("static ").Append(' ');
            if (IsAsync)
                mod.Append("async ").Append(' ');

            var ret = string.IsNullOrWhiteSpace(ReturnType) ? "" : $": {ReturnType}";
            var paramStr = string.Join(", ", Parameters);
            var signature = $"{mod}{Name}({paramStr}){ret}";
            if (!string.IsNullOrWhiteSpace(Body))
            {
                return $"{signature} {Body}";
            }
            // If body missing, end with empty body
            return $"{signature} {{}}";
        }
    }
}
