namespace DotNetVisualLab.Web.Labs.ExportStrategy;

public interface IExportStrategy
{
    string Format { get; }

    ExportResult Export(
        IReadOnlyList<DocumentRecord> documents);
}