using System.Collections.Generic;
using TypescriptParser.Models;

namespace TypescriptParser
{
    /// <summary>
    /// Interface for a lightweight TypeScript parser library.
    /// </summary>
    public interface ITypescriptParser
    {
        // PUBLIC_INTERFACE
        ClassDefinition Parse(string source);

        // PUBLIC_INTERFACE
        List<VariableDefinition> Variables(string source);

        // PUBLIC_INTERFACE
        string ToString(ClassDefinition cls);
    }
}
