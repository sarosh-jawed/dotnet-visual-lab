using DotNetVisualLab.Web.Labs.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetVisualLab.Tests;

public class DependencyInjectionLifetimeTests
{
    [Fact]
    public void Run_WithinOneScope_UsesExpectedLifetimeRules()
    {
        using var provider = BuildProvider();
        using var circuit = provider.CreateScope();
        var runner = circuit.ServiceProvider.GetRequiredService<LifetimeExperimentRunner>();

        var result = runner.Run();

        Assert.NotEqual(result.Transient.First, result.Transient.Second);
        Assert.Equal(result.Scoped.First, result.Scoped.Second);
        Assert.Equal(result.Singleton.First, result.Singleton.Second);
    }

    [Fact]
    public void Run_AcrossTwoScopes_UsesExpectedLifetimeRules()
    {
        using var provider = BuildProvider();
        using var circuit = provider.CreateScope();
        var runner = circuit.ServiceProvider.GetRequiredService<LifetimeExperimentRunner>();

        var firstRequest = runner.Run();
        var secondRequest = runner.Run();

        Assert.NotEqual(firstRequest.Transient.First, secondRequest.Transient.First);
        Assert.NotEqual(firstRequest.Scoped.First, secondRequest.Scoped.First);
        Assert.Equal(firstRequest.Singleton.First, secondRequest.Singleton.First);

        Assert.Equal(1, firstRequest.RunNumber);
        Assert.Equal(2, secondRequest.RunNumber);
    }

    [Fact]
    public void Reset_RestartsSequence_AndPreservesApplicationSingleton()
    {
        using var provider = BuildProvider();
        using var circuit = provider.CreateScope();
        var runner = circuit.ServiceProvider.GetRequiredService<LifetimeExperimentRunner>();

        var firstRequest = runner.Run();
        var secondRequest = runner.Run();

        runner.Reset();
        var afterReset = runner.Run();

        Assert.Equal(1, firstRequest.RunNumber);
        Assert.Equal(2, secondRequest.RunNumber);
        Assert.Equal(1, afterReset.RunNumber);
        Assert.NotEqual(secondRequest.Scoped.First, afterReset.Scoped.First);
        Assert.Equal(secondRequest.Singleton.First, afterReset.Singleton.First);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddTransient<ITransientProbe, TransientProbe>();
        services.AddScoped<IScopedProbe, ScopedProbe>();
        services.AddSingleton<ISingletonProbe, SingletonProbe>();
        services.AddScoped<LifetimeExperimentRunner>();

        return services.BuildServiceProvider();
    }
}
