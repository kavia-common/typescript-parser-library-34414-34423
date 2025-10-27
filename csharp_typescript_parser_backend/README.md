# C# TypeScript Parser Backend

Minimal C# API that parses TypeScript files for imports, classes (name, extends, implements, fields, methods), and top-level functions. Exposes:
- POST /api/parse
- POST /api/variables
- POST /api/tostring

Swagger UI is available at /docs (port 3001 in Development profile).

## Run
- dotnet run
- Open http://localhost:3001/docs

## Endpoints

- POST /api/parse
  Request:
  {
    "source": "import ...; class A extends B implements C { ... }"
  }
  Response: structured imports, classes, functions, diagnostics.

- POST /api/variables
  Request:
  {
    "source": "class A { public foo: string = 'bar'; }"
  }
  Response: array of field declarations.

- POST /api/tostring
  Request accepts either `source` or `parseResult` and optional `className`:
  {
    "source": "class A { x:number=1; }",
    "className": "A"
  }
  or
  {
    "parseResult": { ... },
    "className": "A"
  }
  Response: regenerated TypeScript code string.

## Notes
- This is a lightweight parser, not a full TS grammar implementation.
- It handles simple imports, single class per file, common field/method forms, and top-level functions.
- Diagnostics are included for unrecognized fragments.
