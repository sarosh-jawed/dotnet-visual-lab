# DotNetScope - .NET Visual Lab

Interactive experiments that make real .NET runtime behavior visible.

## Lab 01 - Dependency Injection Lifetimes

Lab 01 turns ASP.NET Core dependency injection into a visual experiment.

Each run creates a fresh `IServiceScope`, resolves Transient, Scoped, and Singleton
services twice, and shows the real GUIDs produced by the built-in .NET DI container.

The UI then compares:

- two resolutions inside the same scope
- the same lifetime across consecutive scopes
- your prediction versus the actual runtime result

### Expected behavior

| Lifetime | Within one scope | Across scopes |
| --- | --- | --- |
| Transient | New instance per resolution | New |
| Scoped | Same instance | New |
| Singleton | Same instance | Same |

## Why this exists

Reading `AddTransient`, `AddScoped`, and `AddSingleton` is easy. Building a mental
model for when instances are actually created and reused is harder.

DotNetScope makes that runtime behavior observable instead of memorized.

## Tech

- .NET 10
- ASP.NET Core
- Blazor Interactive Server
- C#
- xUnit
- GitHub Actions CI

## Local run

```powershell
dotnet restore DotNetVisualLab.slnx
dotnet test DotNetVisualLab.slnx --configuration Release
dotnet run --project src/DotNetVisualLab.Web
```

## CI

Every push to `main` and every pull request targeting `main` runs:

1. restore
2. release build
3. tests

That gives the project an independent build/test gate outside the developer machine.

## Roadmap

- Lab 01: DI lifetimes
- Lab 02: middleware pipeline ordering
- Lab 03: async/await execution flow
- Lab 04: EF Core tracking vs no-tracking
- Lab 05: caching behavior

New labs are added only after the underlying concept is learned and tested.
