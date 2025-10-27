using System.Collections.Generic;
using TypescriptParser.Models;
using TypescriptParser.Parser;
using TypescriptParser.Serialization;

namespace TypescriptParser
{
    /// <summary>
    /// Facade implementation exposing Parse, Variables, and ToString.
    /// </summary>
    public sealed class TypescriptParserFacade : ITypescriptParser
    {
        private readonly TypescriptClassParser _parser = new TypescriptClassParser();

        // PUBLIC_INTERFACE
        public ClassDefinition Parse(string source)
        {
            return _parser.Parse(source);
        }

        // PUBLIC_INTERFACE
        public List<VariableDefinition> Variables(string source)
        {
            var cls = _parser.Parse(source);
            return cls.Variables;
        }

        // PUBLIC_INTERFACE
        public string ToString(ClassDefinition cls)
        {
            return TypescriptClassSerializer.ToSource(cls);
        }
    }
}
