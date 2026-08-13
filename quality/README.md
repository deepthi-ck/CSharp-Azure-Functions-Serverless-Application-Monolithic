# Quality tools — C# Azure Functions Serverless Application Scenario 1

All 12 alt tools live here. Shared configs in `config/`, runners in `scripts/`, outputs in `reports/<tool>/`.

MiniCover coverage XML is the input to diff-cover. Tools analyze real C# in `src/` and `tests/`. Missing tool binaries fail closed (do not skip C# analysis as PASS).
