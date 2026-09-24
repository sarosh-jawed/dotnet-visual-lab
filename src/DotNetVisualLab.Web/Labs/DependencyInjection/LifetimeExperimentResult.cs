namespace DotNetVisualLab.Web.Labs.DependencyInjection;

public sealed record LifetimeResolution(Guid First, Guid Second)
{
    public bool SameWithinScope => First == Second;
}

public sealed record LifetimeExperimentResult(
    int RunNumber,
    DateTimeOffset CapturedAtUtc,
    LifetimeResolution Transient,
    LifetimeResolution Scoped,
    LifetimeResolution Singleton);
