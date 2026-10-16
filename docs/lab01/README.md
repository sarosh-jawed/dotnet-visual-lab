# Lab 01: Dependency Injection Lifetimes

Lab 01 is an interactive .NET 10 exercise showing how the built-in dependency injection container treats Transient, Scoped, and Singleton registrations. A fresh `IServiceScope` is created for each experiment run.

![Lab 01 overview](../images/lab01-overview.png)

## Learning goals

- Predict whether two resolutions in one scope refer to the same instance.
- Compare the same lifetime across separate scopes.
- Confirm results using real GUID-backed service instances, not simulated identity values.

| Lifetime | Repeated resolution in one scope | New scope |
| --- | --- | --- |
| Transient | New instance | New instance |
| Scoped | Same instance | New instance |
| Singleton | Same instance | Same instance |

## Implementation

The lab uses `LifetimeExperimentRunner` and the built-in ASP.NET Core DI container. The service contracts, probes, and runner live in `src/DotNetVisualLab.Web/Labs/DependencyInjection/`; the interactive UI is `Components/Pages/Home.razor`. Its behavior is covered in `tests/DotNetVisualLab.Tests/DependencyInjectionLifetimeTests.cs`.

## Run locally

From the repository root:

```powershell
dotnet run --project src/DotNetVisualLab.Web
```

Open the displayed local URL at `/`. For the full build and validation gate, see the [project README](../../README.md).
