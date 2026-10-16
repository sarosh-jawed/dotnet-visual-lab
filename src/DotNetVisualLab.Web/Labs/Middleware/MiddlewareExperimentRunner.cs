using System.Collections.ObjectModel;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetVisualLab.Web.Labs.Middleware;

public sealed class MiddlewareExperimentRunner(IServiceScopeFactory scopeFactory)
{
    private const string StampHeader = "X-Lab-Stamp";
    private const string StampValue = "executed";
    private const string JsonContentType = "application/json; charset=utf-8";
    private const string AllowedBody = "{\"message\":\"Demo endpoint reached\"}";
    private const string RejectedBody = "{\"message\":\"Rejected by demo gate\"}";

    public async Task<MiddlewareExperimentResult> RunAsync(
        MiddlewareExperimentOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Order is not PipelineOrder.GateBeforeStamp
            and not PipelineOrder.StampBeforeGate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options), options.Order, "Unsupported pipeline order.");
        }

        // Every experiment uses its own DI scope, response stream, context and event list.
        using var scope = scopeFactory.CreateScope();
        using var responseStream = new MemoryStream();

        var app = new ApplicationBuilder(scope.ServiceProvider);
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/demo/documents";
        context.Response.Body = responseStream;

        var runId = Guid.NewGuid();
        var events = new List<MiddlewareTraceEvent>();
        var endpointExecutionCount = 0;

        MiddlewareResponseSnapshot CaptureSnapshot()
        {
            // Read-only wrappers alone are not snapshots: copy the headers first.
            var headers = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var header in context.Response.Headers)
            {
                headers[header.Key] = header.Value.ToString();
            }

            return new MiddlewareResponseSnapshot(
                context.Response.StatusCode,
                new ReadOnlyDictionary<string, string>(headers),
                Encoding.UTF8.GetString(responseStream.ToArray()),
                endpointExecutionCount);
        }

        void Record(string code)
        {
            events.Add(new MiddlewareTraceEvent(
                events.Count + 1, code, CaptureSnapshot()));
        }

        async Task RequestLog(HttpContext request, RequestDelegate next)
        {
            Record("A.before");
            await next(request);
            Record("A.after");
        }

        async Task DemoGate(HttpContext request, RequestDelegate next)
        {
            Record("B.before");

            if (options.RejectRequest)
            {
                request.Response.StatusCode = StatusCodes.Status403Forbidden;
                request.Response.ContentType = JsonContentType;
                await request.Response.WriteAsync(RejectedBody);
                Record("B.reject");
                return; // No next: neither later middleware nor the endpoint can run.
            }

            await next(request);
            Record("B.after");
        }

        async Task ResponseStamp(HttpContext request, RequestDelegate next)
        {
            request.Response.Headers[StampHeader] = StampValue;
            Record("C.before");
            await next(request);
            Record("C.after");
        }

        async Task DemoEndpoint(HttpContext request)
        {
            endpointExecutionCount++;
            request.Response.StatusCode = StatusCodes.Status200OK;
            request.Response.ContentType = JsonContentType;
            await request.Response.WriteAsync(AllowedBody);
            Record("Endpoint.execute");
        }

        app.Use((HttpContext request, RequestDelegate next) =>
            RequestLog(request, next));

        if (options.Order == PipelineOrder.GateBeforeStamp)
        {
            app.Use((HttpContext request, RequestDelegate next) =>
                DemoGate(request, next));
            app.Use((HttpContext request, RequestDelegate next) =>
                ResponseStamp(request, next));
        }
        else
        {
            app.Use((HttpContext request, RequestDelegate next) =>
                ResponseStamp(request, next));
            app.Use((HttpContext request, RequestDelegate next) =>
                DemoGate(request, next));
        }

        app.Run(DemoEndpoint);

        // Build only composes delegates; invoking the result generates real trace events.
        RequestDelegate execute = app.Build();
        await execute(context);

        return new MiddlewareExperimentResult(
            runId,
            options,
            Array.AsReadOnly(events.ToArray()),
            CaptureSnapshot());
    }
}
