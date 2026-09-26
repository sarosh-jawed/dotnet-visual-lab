namespace DotNetVisualLab.Web.Labs.ExportStrategy;

public sealed record DocumentRecord(
    string DocumentId,
    string Title,
    string Department,
    int Pages);