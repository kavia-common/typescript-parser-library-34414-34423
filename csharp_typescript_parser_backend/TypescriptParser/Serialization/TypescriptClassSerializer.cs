using System.Linq;
using System.Text;
using TypescriptParser.Models;

namespace TypescriptParser.Serialization
{
    /// <summary>
    /// Serializes ClassDefinition back to TypeScript code.
    /// </summary>
    internal static class TypescriptClassSerializer
    {
        public static string ToSource(ClassDefinition cls)
        {
            var sb = new StringBuilder();

            // Imports
            if (cls.Imports.Any())
            {
                foreach (var imp in cls.Imports)
                {
                    sb.AppendLine(imp.ToString());
                }
                sb.AppendLine();
            }

            // Class signature
            var extendsPart = string.IsNullOrWhiteSpace(cls.BaseClass) ? "" : $" extends {cls.BaseClass}";
            var implPart = cls.ImplementedInterfaces.Count == 0 ? "" : $" implements {string.Join(", ", cls.ImplementedInterfaces)}";
            sb.Append("export class ").Append(cls.ClassName).Append(extendsPart).Append(implPart).AppendLine(" {");

            // Variables
            foreach (var v in cls.Variables)
            {
                sb.Append("  ").AppendLine(v.ToString());
            }
            if (cls.Variables.Count > 0 && cls.Functions.Count > 0)
                sb.AppendLine();

            // Methods
            foreach (var f in cls.Functions)
            {
                var lines = f.ToString().Split('\n');
                foreach (var line in lines)
                {
                    if (line.Length == 0)
                        sb.AppendLine();
                    else
                        sb.Append("  ").AppendLine(line);
                }
                sb.AppendLine();
            }

            sb.AppendLine("}");
            return sb.ToString().TrimEnd() + "\n";
        }
    }
}
