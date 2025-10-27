# typescript-parser-library-34414-34423

This workspace contains a C# minimal API that parses TypeScript code into basic structures (imports, classes, variables, functions), and regenerates TypeScript text.

- Run: dotnet run (from csharp_typescript_parser_backend)
- Swagger: http://localhost:3001/docs

Endpoints:
- POST /api/parse
- POST /api/variables
- POST /api/tostring
