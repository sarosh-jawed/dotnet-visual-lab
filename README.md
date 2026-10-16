# DotNetScope - .NET Visual Lab

Interactive .NET experiments that make runtime behavior and design patterns visible.

DotNetScope is a learning-first portfolio project. Each lab focuses on one concept, asks the learner to predict what will happen, executes real .NET code, and then visualizes the actual result. The goal is to replace memorized definitions with observable behavior.

Read the [documentation index](docs/README.md) or follow the individual lab guides for implementation details and screenshots.

## Visual preview

### Lab 01 - Dependency Injection Lifetimes

[Read the Lab 01 guide](docs/lab01/README.md)

[![Lab 01 - Dependency Injection Lifetimes](docs/images/lab01-overview.png)](docs/images/lab01-overview.png)

### Lab 02 - Strategy Pattern: Document Export Router

[Read the Lab 02 guide](docs/lab02/README.md)

[![Lab 02 - Strategy Pattern](docs/images/lab02-overview.png)](docs/images/lab02-overview.png)

The Lab 02 page is intentionally longer because it includes the live routing pipeline, prediction feedback, before/after design comparison, and the Open/Closed proof. The full-resolution Lab 02 page capture is available separately:

[Open the full Lab 02 page capture](docs/images/lab02-full.png)

### Lab 03 - Middleware Flight Recorder

[![Lab 03 - Middleware Flight Recorder](docs/lab03/screenshots/lab03-01-hero-overview.png)](docs/lab03/screenshots/lab03-01-hero-overview.png)

[Read the Lab 03 guide, experiment scenarios, and screenshots](docs/lab03/README.md)

## Lab 01 - Dependency Injection Lifetimes

Lab 01 turns ASP.NET Core dependency injection lifetimes into a visual experiment.

Each run creates a fresh `IServiceScope`, resolves Transient, Scoped, and Singleton services twice, and displays the real GUID-backed instances produced by the built-in .NET dependency injection container.

The UI compares:

- two resolutions inside the same scope
- the same lifetime across consecutive scopes
- your prediction versus the actual runtime result
- the behavior at the request/scope boundary

### Expected behavior

| Lifetime | Within one scope | Across scopes |
| --- | --- | --- |
| Transient | New instance per resolution | New |
| Scoped | Same instance | New |
| Singleton | Same instance | Same |

The visualizer uses real services and real container resolutions. The displayed instance IDs are not simulated animation data.

## Lab 02 - Strategy Pattern: Document Export Router

Lab 02 visualizes how the Strategy pattern can replace growing format-specific branching with interchangeable implementations.

The application registers two production strategies:

- `CsvExportStrategy`
- `JsonExportStrategy`

Both implement:

```csharp
IExportStrategy
```

The built-in .NET DI container supplies the available implementations to:

```csharp
ExportRouter(IEnumerable<IExportStrategy> strategies)
```

The router indexes the injected strategies by their declared format and delegates the export to the matching implementation. It does not construct concrete strategies and does not query `IServiceProvider`.

The interactive lab lets you:

- choose CSV, JSON, or intentionally unsupported XML
- predict which strategy will execute before routing
- watch the request move through the real routing pipeline
- inspect the actual generated CSV or JSON payload
- observe the real unsupported-format failure path
- compare a growing `switch` with the Strategy-based design
- see prediction feedback after execution

### Open/Closed proof

The test project defines a third `FakeExportStrategy` that production code does not know about.

`ExportRouter` can execute that strategy without being modified. The test demonstrates that the router depends on the `IExportStrategy` abstraction and collection rather than hardcoded concrete classes.

### Lab 02 behavior covered by tests

- CSV strategy selection and output
- JSON strategy selection and output
- case-insensitive routing
- whitespace-trimmed format input
- blank-format rejection
- null-document rejection
- valid output for an empty document collection
- CSV escaping for commas, quotes, and line breaks
- explicit unsupported-format failure
- duplicate format registration failing fast
- test-only third-strategy extensibility

## Lab 03 - Middleware Flight Recorder

Lab 03 makes ASP.NET Core middleware order, short-circuiting, and the return path observable in an isolated in-memory experiment. The interactive lesson uses an airport checkpoint analogy and executes real .NET delegates against a synthetic request.

The four responsibilities are RequestLog (A), DemoGate (B), ResponseStamp (C), and a terminal demo endpoint (E). You can choose either `A -> B -> C -> Endpoint` or `A -> C -> B -> Endpoint`, then enable or disable a deliberate HTTP 403 rejection at DemoGate.

Before executing, predict whether the endpoint will run, whether `A.after` will run, and whether the final response will contain `X-Lab-Stamp`. After execution, inspect the captured events, response status, body, headers, and C# excerpts. Replay saved events or compare two completed runs without triggering extra requests.

The key comparison is G2 versus S2. Both respond with HTTP 403 and do not reach the endpoint. Only S2 retains the informational stamp header because C runs before B rejects.

The lab also includes beginner explanations, deeper technical notes, reflection questions, and six dedicated middleware tests. **The pipeline is synthetic and in memory. It does not trace live Kestrel traffic or implement production security.**

[Open the full Lab 03 guide](docs/lab03/README.md) | [View the G2 versus S2 comparison](docs/lab03/screenshots/lab03-06-comparison-panel.png)

## Why this exists

Reading framework APIs and design-pattern definitions is easier than building a durable mental model of what the runtime is actually doing.

DotNetScope follows a simple loop:

1. understand the concept
2. predict the behavior
3. execute real code
4. inspect the result
5. test the implementation
6. explain why the result occurred

Each lab stays deliberately narrow so the visualization teaches one concept instead of hiding it behind unrelated infrastructure.

## Tech

- .NET 10
- ASP.NET Core
- Blazor Interactive Server
- C#
- built-in .NET dependency injection
- `System.Text.Json`
- xUnit
- GitHub Actions CI

## Local run

```powershell
dotnet restore DotNetVisualLab.slnx
dotnet test DotNetVisualLab.slnx --configuration Release
dotnet run --project src\DotNetVisualLab.Web
```

Then open:

- `/` for Lab 01
- /strategy for Lab 02
- /middleware for Lab 03

## Validation

The October 16, 2026 Lab 03 merge has 19 passing automated tests across Labs 01, 02, and 03 (including six focused middleware tests). The main-branch GitHub Actions workflow also passed for the merge.

The delivery gate for each lab is:

```powershell
dotnet build DotNetVisualLab.slnx --configuration Release
dotnet test DotNetVisualLab.slnx --configuration Release
git diff --check
```

The UI is also manually exercised through its supported prediction and runtime paths before the feature is merged.

## Engineering workflow

Each lab follows the same delivery discipline:

1. learn and explain the concept
2. define the smallest useful experiment
3. implement real behavior
4. add automated tests and edge cases
5. inspect the interactive UI
6. run the full Release validation gate
7. open a feature pull request
8. require green GitHub Actions CI
9. squash merge
10. sync `main` and remove the feature branch

## CI

Every push to `main` and every pull request targeting `main` runs:

1. restore
2. Release build
3. tests

This provides an independent validation gate outside the developer machine.


## Docker - Local Run

Prerequisites: Docker Desktop with the Linux engine running.

Run from the repository root in PowerShell:

```powershell
docker build -t dotnet-visual-lab:verify .
docker run -d --name dotnet-visual-lab-verify -p 8085:8080 dotnet-visual-lab:verify

Start-Sleep -Seconds 10
(Invoke-WebRequest -UseBasicParsing http://localhost:8085/health).StatusCode

docker restart dotnet-visual-lab-verify
Start-Sleep -Seconds 10
(Invoke-WebRequest -UseBasicParsing http://localhost:8085/health).StatusCode

docker logs dotnet-visual-lab-verify --tail 30
```

Expected: HTTP 200 and Healthy before and after restart.

Open:
- http://localhost:8085/  -  Dependency Injection lab
- http://localhost:8085/strategy - Strategy Pattern lab
- http://localhost:8085/middleware - Middleware Flight Recorder lab

Port 8085 is on the host; port 8080 is inside the container.

These commands document local Docker verification. Docker was verified before Lab 03; the new Lab 03 Docker route is documented here but has not been separately reverified after the merge. This is not cloud hosting or production readiness. Production deployment would require explicit HTTPS configuration, persistent and protected ASP.NET Core Data Protection keys, secure secrets management, and operational controls. Do not include credentials or secrets in the image or repository.

When finished:

```powershell
docker stop dotnet-visual-lab-verify
docker rm dotnet-visual-lab-verify
```


## Roadmap

- Lab 01: Dependency Injection lifetimes (complete)
- Lab 02: Strategy Pattern / document export routing (complete)
- Lab 03: Middleware Flight Recorder (complete)
- Lab 04: Async/await execution flow
- Lab 05: EF Core tracking vs no-tracking
- Lab 06: Caching behavior

New labs are added only after the underlying concept is learned and tested.
