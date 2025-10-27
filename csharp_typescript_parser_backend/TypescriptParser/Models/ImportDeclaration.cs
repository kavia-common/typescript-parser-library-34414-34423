using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypescriptParser.Models
{
    /// <summary>
    /// Represents a single TypeScript import declaration.
    /// </summary>
    public sealed class ImportDeclaration
    {
        /// <summary>
        /// Default import identifier. Example: import React from 'react' => React
        /// </summary>
        public string? DefaultImport { get; set; }

        /// <summary>
        /// Named imports. Example: import { useState, useEffect as effect } from 'react';
        /// </summary>
        public List<string> NamedImports { get; } = new List<string>();

        /// <summary>
        /// Namespace import. Example: import * as React from 'react' => React
        /// </summary>
        public string? NamespaceImport { get; set; }

        /// <summary>
        /// The module specifier (path or package). Example: 'react'
        /// </summary>
        public string FromModule { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a type-only import. Example: import type { Foo } from 'bar'
        /// </summary>
        public bool IsTypeOnly { get; set; }

        /// <summary>
        /// Raw captured source line for debugging (optional).
        /// </summary>
        public string? Raw { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("import ");
            if (IsTypeOnly)
            {
                sb.Append("type ");
            }

            var hasDefault = !string.IsNullOrWhiteSpace(DefaultImport);
            var hasNamespace = !string.IsNullOrWhiteSpace(NamespaceImport);
            var hasNamed = NamedImports.Count > 0;

            var wroteAny = false;

            if (hasDefault)
            {
                sb.Append(DefaultImport);
                wroteAny = true;
            }

            if (hasNamespace)
            {
                if (wroteAny) sb.Append(", ");
                sb.Append("* as ").Append(NamespaceImport);
                wroteAny = true;
            }

            if (hasNamed)
            {
                if (wroteAny) sb.Append(", ");
                sb.Append("{ ").Append(string.Join(", ", NamedImports)).Append(" }");
                wroteAny = true;
            }

            if (!wroteAny)
            {
                // Bare import: import 'module';
                sb.Append('\'').Append(FromModule).Append('\'').Append(';');
                return sb.ToString();
            }

            sb.Append(" from '").Append(FromModule).Append("';");
            return sb.ToString();
        }
    }
}
