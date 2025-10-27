using System;

namespace TypescriptParser.Models
{
    /// <summary>
    /// Represents a TypeScript class property/field (variable definition).
    /// </summary>
    public sealed class VariableDefinition
    {
        /// <summary>
        /// Variable name as declared.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional explicit type if present in the source (string inside ': ...').
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Optional initializer/expression as text (string after '= ...').
        /// </summary>
        public string? Initializer { get; set; }

        /// <summary>
        /// Optional access modifier: public, private, protected, or undefined.
        /// </summary>
        public string? AccessModifier { get; set; }

        /// <summary>
        /// True if 'readonly' keyword is present.
        /// </summary>
        public bool IsReadonly { get; set; }

        /// <summary>
        /// True if 'static' keyword is present.
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// True if '?' optional property marker present.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Original source snippet (for reference/debug).
        /// </summary>
        public string? Raw { get; set; }

        public override string ToString()
        {
            var mod = string.Empty;
            if (!string.IsNullOrWhiteSpace(AccessModifier))
            {
                mod += AccessModifier + " ";
            }
            if (IsStatic) mod += "static ";
            if (IsReadonly) mod += "readonly ";

            var namePart = Name + (IsOptional ? "?" : "");
            var typePart = string.IsNullOrWhiteSpace(Type) ? "" : $": {Type}";
            var initPart = string.IsNullOrWhiteSpace(Initializer) ? "" : $" = {Initializer}";
            return $"{mod}{namePart}{typePart}{initPart};";
        }
    }
}
