using System.Text.Json;

namespace DotNetVisualLab.Web.Labs.ExportStrategy;

public sealed class JsonExportStrategy : IExportStrategy
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

    public string Format => "json";

    public ExportResult Export(
        IReadOnlyList<DocumentRecord> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var json = JsonSerializer.Serialize(
            documents,
            SerializerOptions);

        return new ExportResult(
            Format,
            "application/json",
            json);
    }
}