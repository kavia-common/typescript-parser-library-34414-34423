#!/bin/bash
cd /home/kavia/workspace/code-generation/typescript-parser-library-34414-34423/csharp_typescript_parser_backend
dotnet build --no-restore -v quiet -nologo -consoleloggerparameters:NoSummary /p:TreatWarningsAsErrors=false
LINT_EXIT_CODE=$?
if [ $LINT_EXIT_CODE -ne 0 ]; then
  exit 1
fi

