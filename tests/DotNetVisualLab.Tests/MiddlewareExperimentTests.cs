using DotNetVisualLab.Web.Labs.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetVisualLab.Tests;

public class MiddlewareExperimentTests
{
    [Theory]
    [InlineData(PipelineOrder.GateBeforeStamp, false)]
    [InlineData(PipelineOrder.GateBeforeStamp, true)]
    [InlineData(PipelineOrder.StampBeforeGate, false)]
    [InlineData(PipelineOrder.StampBeforeGate, true)]
    public async Task RunAsync_ExecutesConfiguredPipeline(
        PipelineOrder order, bool rejectRequest)
    {
        using var provider = BuildProvider();
        var runner = provider.GetRequiredService<MiddlewareExperimentRunner>();
        var options = new MiddlewareExperimentOptions(rejectRequest, order);

        var result = await runner.RunAsync(options);

        string[] expectedCodes = (order, rejectRequest) switch
        {
            (PipelineOrder.GateBeforeStamp, false) =>
            [
                "A.before", "B.before", "C.before", "Endpoint.execute",
                "C.after", "B.after", "A.after"
            ],
            (PipelineOrder.GateBeforeStamp, true) =>
            [
                "A.before", "B.before", "B.reject", "A.after"
            ],
            (PipelineOrder.StampBeforeGate, false) =>
            [
                "A.before", "C.before", "B.before", "Endpoint.execute",
                "B.after", "C.after", "A.after"
            ],
            (PipelineOrder.StampBeforeGate, true) =>
            [
                "A.before", "C.before", "B.before", "B.reject",
                "C.after", "A.after"
            ],
            _ => throw new InvalidOperationException("Unexpected theory data.")
        };

        Assert.Equal(expectedCodes, result.Events.Select(e => e.Code).ToArray());
        Assert.Equal(
            Enumerable.Range(1, expectedCodes.Length).ToArray(),
            result.Events.Select(e => e.Sequence).ToArray());

        Assert.Equal(rejectRequest ? 403 : 200, result.Response.StatusCode);
        Assert.Equal(
            rejectRequest
                ? "{\"message\":\"Rejected by demo gate\"}"
                : "{\"message\":\"Demo endpoint reached\"}",
            result.Response.Body);
        Assert.Equal(rejectRequest ? 0 : 1, result.Response.EndpointExecutionCount);

        Assert.True(result.Response.Headers.TryGetValue(
            "content-type", out var contentType));
        Assert.Equal("application/json; charset=utf-8", contentType);

        var expectedStamp = !rejectRequest || order == PipelineOrder.StampBeforeGate;
        var hasStamp = result.Response.Headers.TryGetValue(
            "x-lab-stamp", out var stamp);
        Assert.Equal(expectedStamp, hasStamp);
        if (expectedStamp)
        {
            Assert.Equal("executed", stamp);
        }
        else
        {
            Assert.False(result.Response.Headers.ContainsKey("X-Lab-Stamp"));
        }

        Assert.Equal(options, result.Options);
        Assert.NotEqual(Guid.Empty, result.RunId);
    }

    [Fact]
    public async Task RunAsync_PreservesEarlierSnapshots()
    {
        using var provider = BuildProvider();
        var runner = provider.GetRequiredService<MiddlewareExperimentRunner>();
        var result = await runner.RunAsync(new(false, PipelineOrder.GateBeforeStamp));

        var beforeA = Assert.Single(result.Events, e => e.Code == "A.before");
        var beforeC = Assert.Single(result.Events, e => e.Code == "C.before");
        var endpoint = Assert.Single(result.Events, e => e.Code == "Endpoint.execute");

        Assert.False(beforeA.Snapshot.Headers.ContainsKey("X-Lab-Stamp"));
        Assert.Equal(string.Empty, beforeA.Snapshot.Body);
        Assert.Equal(0, beforeA.Snapshot.EndpointExecutionCount);

        Assert.True(beforeC.Snapshot.Headers.TryGetValue("X-Lab-Stamp", out var stamp));
        Assert.Equal("executed", stamp);
        Assert.Equal(string.Empty, beforeC.Snapshot.Body);
        Assert.Equal(0, beforeC.Snapshot.EndpointExecutionCount);

        Assert.Equal("{\"message\":\"Demo endpoint reached\"}", endpoint.Snapshot.Body);
        Assert.Equal(1, endpoint.Snapshot.EndpointExecutionCount);
        Assert.Equal("executed", result.Response.Headers["X-Lab-Stamp"]);

        // Verify a later header mutation did not change the early captured state.
        Assert.False(beforeA.Snapshot.Headers.ContainsKey("X-Lab-Stamp"));
        Assert.Equal(string.Empty, beforeA.Snapshot.Body);

        var dictionaryView = (IDictionary<string, string>)beforeC.Snapshot.Headers;
        Assert.Throws<NotSupportedException>(() =>
            dictionaryView["Unexpected"] = "mutation");
    }

    [Fact]
    public async Task RunAsync_KeepsExecutionsIndependent()
    {
        using var provider = BuildProvider();
        var runner = provider.GetRequiredService<MiddlewareExperimentRunner>();

        var first = await runner.RunAsync(new(false, PipelineOrder.GateBeforeStamp));
        var second = await runner.RunAsync(new(true, PipelineOrder.GateBeforeStamp));

        Assert.NotEqual(first.RunId, second.RunId);
        Assert.Equal(1, second.Events[0].Sequence);
        Assert.Equal(4, second.Events.Count);
        Assert.Equal(403, second.Response.StatusCode);
        Assert.Equal(0, second.Response.EndpointExecutionCount);
        Assert.False(second.Response.Headers.ContainsKey("X-Lab-Stamp"));

        Assert.Equal(7, first.Events.Count);
        Assert.Equal("A.before", first.Events[0].Code);
        Assert.Equal("A.after", first.Events[^1].Code);
        Assert.Equal(200, first.Response.StatusCode);
        Assert.Equal(1, first.Response.EndpointExecutionCount);
        Assert.Equal("{\"message\":\"Demo endpoint reached\"}", first.Response.Body);
        Assert.Equal("executed", first.Response.Headers["X-Lab-Stamp"]);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddTransient<MiddlewareExperimentRunner>();
        return services.BuildServiceProvider();
    }
}
