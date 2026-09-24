using Microsoft.Extensions.DependencyInjection;

namespace DotNetVisualLab.Web.Labs.DependencyInjection;

/// <summary>
/// Creates a fresh IServiceScope for every run, then resolves each lifetime twice.
/// A fresh scope has the same DI lifetime boundary semantics that ASP.NET Core
/// uses for request-scoped services.
/// </summary>
public sealed class LifetimeExperimentRunner(IServiceScopeFactory scopeFactory)
{
    private int _runNumber;

    public LifetimeExperimentResult Run()
    {
        using var scope = scopeFactory.CreateScope();
        var services = scope.ServiceProvider;

        var transient = new LifetimeResolution(
            services.GetRequiredService<ITransientProbe>().InstanceId,
            services.GetRequiredService<ITransientProbe>().InstanceId);

        var scoped = new LifetimeResolution(
            services.GetRequiredService<IScopedProbe>().InstanceId,
            services.GetRequiredService<IScopedProbe>().InstanceId);

        var singleton = new LifetimeResolution(
            services.GetRequiredService<ISingletonProbe>().InstanceId,
            services.GetRequiredService<ISingletonProbe>().InstanceId);

        return new LifetimeExperimentResult(
            RunNumber: Interlocked.Increment(ref _runNumber),
            CapturedAtUtc: DateTimeOffset.UtcNow,
            Transient: transient,
            Scoped: scoped,
            Singleton: singleton);
    }

    public void Reset() => Interlocked.Exchange(ref _runNumber, 0);
}
