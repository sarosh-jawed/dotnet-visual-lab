namespace DotNetVisualLab.Web.Labs.Middleware;

public enum PipelineOrder
{
    GateBeforeStamp,
    StampBeforeGate
}

public sealed record MiddlewareExperimentOptions(
    bool RejectRequest,
    PipelineOrder Order);

public sealed record MiddlewareResponseSnapshot(
    int StatusCode,
    IReadOnlyDictionary<string, string> Headers,
    string Body,
    int EndpointExecutionCount);

public sealed record MiddlewareTraceEvent(
    int Sequence,
    string Code,
    MiddlewareResponseSnapshot Snapshot);

public sealed record MiddlewareExperimentResult(
    Guid RunId,
    MiddlewareExperimentOptions Options,
    IReadOnlyList<MiddlewareTraceEvent> Events,
    MiddlewareResponseSnapshot Response);
