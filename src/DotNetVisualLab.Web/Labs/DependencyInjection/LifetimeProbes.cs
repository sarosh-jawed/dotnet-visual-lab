namespace DotNetVisualLab.Web.Labs.DependencyInjection;

public sealed class TransientProbe : ITransientProbe
{
    public Guid InstanceId { get; } = Guid.NewGuid();
}

public sealed class ScopedProbe : IScopedProbe
{
    public Guid InstanceId { get; } = Guid.NewGuid();
}

public sealed class SingletonProbe : ISingletonProbe
{
    public Guid InstanceId { get; } = Guid.NewGuid();
}
