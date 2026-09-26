using System.Text.Json;
using DotNetVisualLab.Web.Labs.ExportStrategy;

namespace DotNetVisualLab.Tests;

public class ExportStrategyTests
{
    private static readonly IReadOnlyList<DocumentRecord> Documents =
    [
        new(
            "DOC-101",
            "Quarterly Review",
            "Finance",
            18),

        new(
            "DOC-205",
            "Hiring Plan",
            "People Ops",
            7),

        new(
            "DOC-309",
            "Platform Notes",
            "Engineering",
            12)
    ];

    [Fact]
    public void Export_Csv_UsesCsvStrategy()
    {
        var router = CreateRouter(
            new CsvExportStrategy(),
            new JsonExportStrategy());

        var result = router.Export("csv", Documents);

        Assert.Equal("csv", result.Format);
        Assert.Equal("text/csv", result.ContentType);

        Assert.Contains(
            "document_id,title,department,pages",
            result.Content);

        Assert.Contains(
            "DOC-101,Quarterly Review,Finance,18",
            result.Content);
    }

    [Fact]
    public void Export_Json_UsesJsonStrategy()
    {
        var router = CreateRouter(
            new CsvExportStrategy(),
            new JsonExportStrategy());

        var result = router.Export("JSON", Documents);

        Assert.Equal("json", result.Format);
        Assert.Equal("application/json", result.ContentType);

        using var json = JsonDocument.Parse(result.Content);

        Assert.Equal(
            Documents.Count,
            json.RootElement.GetArrayLength());

        Assert.Equal(
            "DOC-101",
            json.RootElement[0]
                .GetProperty("documentId")
                .GetString());
    }

    [Fact]
    public void Export_UnsupportedFormat_ThrowsExplicitError()
    {
        var router = CreateRouter(
            new CsvExportStrategy(),
            new JsonExportStrategy());

        var exception = Assert.Throws<NotSupportedException>(
            () => router.Export("xml", Documents));

        var message = exception.Message.ToLowerInvariant();

        Assert.Contains("xml", message);
        Assert.Contains("csv", message);
        Assert.Contains("json", message);
    }

    [Fact]
    public void Export_TestOnlyStrategy_WorksWithoutChangingRouter()
    {
        var router = CreateRouter(
            new CsvExportStrategy(),
            new JsonExportStrategy(),
            new FakeExportStrategy());

        var result = router.Export("fake", Documents);

        Assert.Equal("fake", result.Format);
        Assert.Equal("text/plain", result.ContentType);
        Assert.Equal(
            $"FAKE EXPORT: {Documents.Count} documents",
            result.Content);
    }

    [Fact]
    public void CsvStrategy_EscapesCommaAndQuotes()
    {
        var strategy = new CsvExportStrategy();

        IReadOnlyList<DocumentRecord> documents =
        [
            new(
                "DOC-999",
                "Review, \"Final\"",
                "Finance",
                4)
        ];

        var result = strategy.Export(documents);

        Assert.Contains(
            "\"Review, \"\"Final\"\"\"",
            result.Content);
    }

    [Fact]
    public void Export_TrimmedMixedCaseFormat_RoutesSuccessfully()
    {
        var router = CreateRouter(
            new CsvExportStrategy(),
            new JsonExportStrategy());

        var result = router.Export("  JsOn  ", Documents);

        Assert.Equal("json", result.Format);
        Assert.Equal("application/json", result.ContentType);
    }

    [Fact]
    public void Export_BlankFormat_ThrowsArgumentException()
    {
        var router = CreateRouter(
            new CsvExportStrategy(),
            new JsonExportStrategy());

        var exception = Assert.Throws<ArgumentException>(
            () => router.Export("   ", Documents));

        Assert.Equal("format", exception.ParamName);
    }

    [Fact]
    public void Export_EmptyDocuments_ReturnsValidEmptyPayloads()
    {
        var router = CreateRouter(
            new CsvExportStrategy(),
            new JsonExportStrategy());

        IReadOnlyList<DocumentRecord> documents = Array.Empty<DocumentRecord>();

        var csv = router.Export("csv", documents);
        var json = router.Export("json", documents);

        Assert.Equal(
            "document_id,title,department,pages",
            csv.Content);

        Assert.Equal("[]", json.Content);
    }

    [Fact]
    public void Export_NullDocuments_ThrowsArgumentNullException()
    {
        var router = CreateRouter(
            new CsvExportStrategy(),
            new JsonExportStrategy());

        Assert.Throws<ArgumentNullException>(
            () => router.Export("csv", null!));
    }

    [Fact]
    public void Constructor_DuplicateFormats_FailsFast()
    {
        Assert.Throws<ArgumentException>(() =>
            new ExportRouter(
                new IExportStrategy[]
                {
                    new CsvExportStrategy(),
                    new DuplicateCsvStrategy()
                }));
    }

    private static ExportRouter CreateRouter(
        params IExportStrategy[] strategies)
    {
        return new ExportRouter(strategies);
    }

    private sealed class FakeExportStrategy : IExportStrategy
    {
        public string Format => "fake";

        public ExportResult Export(
            IReadOnlyList<DocumentRecord> documents)
        {
            return new ExportResult(
                Format,
                "text/plain",
                $"FAKE EXPORT: {documents.Count} documents");
        }
    }

    private sealed class DuplicateCsvStrategy : IExportStrategy
    {
        public string Format => "CSV";

        public ExportResult Export(
            IReadOnlyList<DocumentRecord> documents)
        {
            return new ExportResult(
                Format,
                "text/plain",
                "duplicate");
        }
    }
}
