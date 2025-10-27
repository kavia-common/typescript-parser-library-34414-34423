using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypescriptParser.Models
{
    /// <summary>
    /// Represents a parsed TypeScript class file with imports, class metadata, members, and methods.
    /// </summary>
    public sealed class ClassDefinition
    {
        /// <summary>
        /// The captured class name.
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// Optional base class name if 'extends' is present.
        /// </summary>
        public string? BaseClass { get; set; }

        /// <summary>
        /// Optional list of implemented interface names (implements A, B).
        /// </summary>
        public List<string> ImplementedInterfaces { get; } = new List<string>();

        /// <summary>
        /// All import declarations at the top of the file.
        /// </summary>
        public List<ImportDeclaration> Imports { get; } = new List<ImportDeclaration>();

        /// <summary>
        /// All class variables (properties/fields).
        /// </summary>
        public List<VariableDefinition> Variables { get; } = new List<VariableDefinition>();

        /// <summary>
        /// All class methods.
        /// </summary>
        public List<FunctionDefinition> Functions { get; } = new List<FunctionDefinition>();

        /// <summary>
        /// Raw full source captured (optional, useful for debugging).
        /// </summary>
        public string? RawSource { get; set; }
    }
}
