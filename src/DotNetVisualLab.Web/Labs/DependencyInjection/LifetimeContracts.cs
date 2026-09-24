namespace DotNetVisualLab.Web.Labs.DependencyInjection;

public interface ITransientProbe
{
    Guid InstanceId { get; }
}

public interface IScopedProbe
{
    Guid InstanceId { get; }
}

public interface ISingletonProbe
{
    Guid InstanceId { get; }
}
