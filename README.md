# C# Azure Functions Serverless Application — Scenario 1 Monolithic

Minimal C# Azure Functions serverless monolith: one repo, one customer .NET version per branch, flat single module. HTTP trigger functions and shared app/service logic use the **same** version. This is **not** Scenario 2 (no `CSharp_FE*_BE*` branches) and **not** MAUI, gRPC Service, or Blazor WebAssembly.

Inspired conceptually by [Azure/azure-functions-dotnet-worker](https://github.com/Azure/azure-functions-dotnet-worker). That tree is **not** cloned. Isolated-worker patterns kept: `Program.cs` host bootstrap, HTTP trigger function classes, `host.json` / `local.settings.json`, worker DI-style composition (`FunctionApp`), request/response models, Health / Version / Stats.

C# BCL built-ins (`Dictionary` / `ConcurrentDictionary`, LINQ, `DateTimeOffset` / `TimeSpan`, `string.IsNullOrWhiteSpace`, `Task`, exceptions, `System.Text.Json`) are **required** for PUT/GET/DELETE/TTL/eviction/sample-load. Removing them breaks compile or runtime.

## Layout

- `src/` — flat module: isolated worker `Program.cs`, `Functions/` HTTP triggers, shared store/service
- `tests/` — unit + function→service + `CsharpBuiltinUsageTest`
- `quality/` — 12 alt tools, shared configs/scripts/reports
- `config/`, `data/`, `fixtures/` — one copy each

## Branches (exactly 8)

| Branch | Customer Version | MSBuild TFM |
|---|---|---|
| `C#_net6.0` | 6 | net6.0 |
| `C#_net7.0` | 7 | net7.0 |
| `C#_net8.0` | 8 | net8.0 |
| `C#_net9.0` | 9 | net9.0 |
| `C#_net10.0` | 10 | net10.0 |
| `C#_net42.0` | 42.0 (named) | net462 |
| `C#_net472.0` | 4.7.2 | net472 |
| `C#_net4.8` | 4.8 | net48 |

`net42` is not a shipping TFM; `C#_net42.0` compiles as `net462`. Framework 4.8 branch is **`C#_net4.8`** (not `net48` / `FW48` / `C#_net648.0`); MSBuild TFM is `net48`.

## Build

```bash
python build.py
```

or `./build.sh`

```bash
dotnet run --project src/CSharpFunctionsMonolith.csproj
```

HTTP routes (local test-host equivalent of Azure Functions isolated worker):

- Operator UI (page-to-page): `http://127.0.0.1:5083/`
  - `/` Dashboard
  - `/resources.html` Resource PUT/GET/DELETE
  - `/stats.html` Statistics
  - `/nodes.html` Partition nodes
  - `/health.html` Health
  - `/version.html` Customer version
- `GET /api/health`
- `GET /api/version`
- `GET /api/stats`
- `GET /api/nodes`
- `GET /api/resources`
- `GET /api/resources/{key}`
- `PUT /api/resources/{key}`
- `DELETE /api/resources/{key}`

Default listen: `http://127.0.0.1:5083/`

## Quality tools (all 12, every branch)

unilyze, Dolos, Opengrep, Opengrep code-health, Opengrep input-validation, Opengrep secrets, Opengrep auth, Trivy NuGet/CVE, Trivy, MiniCover, Stryker.NET, diff-cover (MiniCover → diff-cover).

See `quality/README.md` and `docs/version-matrix.md`.
