namespace DotNetVisualLab.Web.Labs.ExportStrategy;

public sealed record ExportResult(
    string Format,
    string ContentType,
    string Content);