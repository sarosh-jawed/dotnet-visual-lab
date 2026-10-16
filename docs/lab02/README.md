# Lab 02: Strategy Pattern - Document Export Router

Lab 02 demonstrates the Strategy pattern through an interactive document export router built with C#, .NET 10, and dependency injection.

![Lab 02 overview](../images/lab02-overview.png)

[View the full Lab 02 page capture](../images/lab02-full.png)

## Learning goals

- Understand why `IExportStrategy` lets the router depend on an abstraction.
- Predict which implementation runs for CSV and JSON.
- Observe the deliberate unsupported XML request.
- See how extending the strategy collection avoids modifying the router's selection logic.

## Implementation

`CsvExportStrategy` and `JsonExportStrategy` implement the same `IExportStrategy` contract. The DI container injects `IEnumerable<IExportStrategy>` into `ExportRouter`. The router selects a registered strategy without constructing concrete implementations or resolving services dynamically.

The implementation lives in `src/DotNetVisualLab.Web/Labs/ExportStrategy/`, its interactive UI is `Components/Pages/Strategy.razor`, and its automated checks are in `tests/DotNetVisualLab.Tests/ExportStrategyTests.cs`. The tests include escaping and invalid input cases, duplicate registrations, and a test-only third implementation.

## Run locally

From the repository root:

```powershell
dotnet run --project src/DotNetVisualLab.Web
```

Open the displayed local URL at `/strategy`. For the full build and validation gate, see the [project README](../../README.md).
