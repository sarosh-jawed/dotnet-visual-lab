namespace DotNetVisualLab.Web.Labs.ExportStrategy;

public sealed class ExportRouter
{
    private readonly IReadOnlyDictionary<string, IExportStrategy> _strategies;

    public ExportRouter(
        IEnumerable<IExportStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        _strategies = strategies.ToDictionary(
            strategy => strategy.Format,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> SupportedFormats =>
        _strategies.Keys.ToArray();

    public ExportResult Export(
        string format,
        IReadOnlyList<DocumentRecord> documents)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException(
                "An export format is required.",
                nameof(format));
        }

        ArgumentNullException.ThrowIfNull(documents);

        var normalizedFormat = format.Trim();

        if (!_strategies.TryGetValue(
                normalizedFormat,
                out var strategy))
        {
            var supportedFormats = string.Join(
                ", ",
                _strategies.Keys.Order());

            throw new NotSupportedException(
                $"Export format '{normalizedFormat}' is not supported. " +
                $"Supported formats: {supportedFormats}.");
        }

        return strategy.Export(documents);
    }
}