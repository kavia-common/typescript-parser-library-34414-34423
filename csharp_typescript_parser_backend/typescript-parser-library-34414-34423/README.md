# typescript-parser-library-34414-34423

A lightweight C# library that parses TypeScript class files to extract:
- Imports
- Class name, base class (extends), implemented interfaces
- Class variables (properties/fields)
- Class methods (signatures and bodies captured via balanced-brace scanning)

It exposes a simple facade with:
- Parse(string): returns a ClassDefinition tree
- Variables(string): returns List<VariableDefinition> quickly
- ToString(ClassDefinition): generates TypeScript code from the model

## Getting Started

The library is part of the `csharp_typescript_parser_backend` project. No new HTTP endpoints are added; it is a library registered in DI (optional).

### Optional DI registration
Program.cs registers the parser:
```csharp
builder.Services.AddSingleton<TypescriptParser.ITypescriptParser, TypescriptParser.TypescriptParserFacade>();
```

### Example usage
See `csharp_typescript_parser_backend/Examples/ExampleUsage.cs`:
```csharp
using TypescriptParser;

string ts = "...";
ITypescriptParser parser = new TypescriptParserFacade();
var model = parser.Parse(ts);
var variables = parser.Variables(ts);
var regenerated = parser.ToString(model);
```

## Design Notes

- Regex + depth scanning approach (not a full TypeScript grammar).
- Balanced brace scanning skips strings and comments, reducing false positives.
- Method parameters are split at top-level commas, ignoring nested generics/tuples.

## Limitations

- Not a full TS parser; complex syntax (decorators, overloads, generics with intricate constraints) may be partially captured.
- Field and method regexes are intentionally conservative; extremely unconventional formatting might be missed.
- Only a single primary `export class ... {}` per file is expected. Multiple classes may not be fully supported.
- Types within method bodies are not analyzed; bodies are treated as text.

## Public API

- ITypescriptParser
  - Parse(string): ClassDefinition
  - Variables(string): List<VariableDefinition>
  - ToString(ClassDefinition): string

All public methods are marked with "PUBLIC_INTERFACE" comments in source as required.

## Build

- Targets net8.0, no additional NuGet dependencies beyond existing NSwag for the host app.
- Compile as usual with `dotnet build` for the backend project.
