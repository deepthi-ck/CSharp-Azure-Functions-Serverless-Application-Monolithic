# Version matrix — C# Azure Functions Serverless Application, Scenario 1 Monolithic

This matrix is **Scenario 1 Azure Functions Serverless Application only**.

- Language: C#
- Priority: 4
- Project Type: Azure Functions Serverless Application
- Scenario: **1 - Monolithic only** (not Scenario 2 split FE/BE)
- Module: flat (single module)
- Not .NET MAUI Cross-Platform Application
- Not gRPC Service
- Not Blazor WebAssembly Application
- C# language built-in functions must be present and **required to run** on every branch
- Branches: exactly 8
- Framework 4.8 branch is **C#_net4.8** (not net48 / FW48 / C#_net648.0)
- Sheet-listed Customer Versions include .NET 6 / .NET 8 / .NET 9 / .NET 10 / .NET Framework 4.8 (plus fill branches net7.0 / net42.0 / net472.0)

Proof Source for all tools: Shivam-C# sheet (Customer's Language Version Support column)

## Branches

| Branch | Customer Version | MSBuild TFM |
|---|---|---|
| C#_net6.0 | 6 | net6.0 |
| C#_net7.0 | 7 | net7.0 |
| C#_net8.0 | 8 | net8.0 |
| C#_net9.0 | 9 | net9.0 |
| C#_net10.0 | 10 | net10.0 |
| C#_net42.0 | 42.0 (named) | net462 |
| C#_net472.0 | 4.7.2 | net472 |
| C#_net4.8 | 4.8 | net48 |

## Tools on every branch (all 12)

| Tool | Tool Verified Version | Derivation Note |
|---|---|---|
| unilyze | 8 | .NET 8–10, C# 8–14 |
| Dolos | N/A (language/runtime-version agnostic) | C# language-version independent (source similarity) |
| Opengrep | 12 | C# 12+ |
| Opengrep (code-health/complexity) | 12 | C# 12+ |
| Opengrep (input-validation/taint) | 12 | C# 12+ |
| Opengrep (secrets/dataflow/taint) | 12 | C# 12+ |
| Opengrep (auth/access-control) | 12 | C# 12+ |
| Trivy (NuGet/CVE/supply-chain) | N/A (language/runtime-version agnostic) | .NET / NuGet |
| Trivy | N/A (language/runtime-version agnostic) | .NET / NuGet |
| MiniCover | 8 | .NET SDK 8/9/10 |
| Stryker.NET | 8 | .NET 8–10, C# 12–14 |
| diff-cover | UNRESOLVED | Not C# version dependent; consumes MiniCover/Cobertura |

Each branch documents: Language=C# | Priority=4 | Project Type=Azure Functions Serverless Application | Scenario=1 - Monolithic | Module=flat (single module) | Customer Version as in the table above | plus every tool row.
