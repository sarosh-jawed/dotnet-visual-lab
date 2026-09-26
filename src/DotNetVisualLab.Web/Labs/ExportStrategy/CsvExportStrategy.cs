using System.Text;

namespace DotNetVisualLab.Web.Labs.ExportStrategy;

public sealed class CsvExportStrategy : IExportStrategy
{
	public string Format => "csv";

	public ExportResult Export(
		IReadOnlyList<DocumentRecord> documents)
	{
		ArgumentNullException.ThrowIfNull(documents);

		var csv = new StringBuilder();

		csv.AppendLine("document_id,title,department,pages");

		foreach (var document in documents)
		{
			csv.Append(Escape(document.DocumentId))
				.Append(',')
				.Append(Escape(document.Title))
				.Append(',')
				.Append(Escape(document.Department))
				.Append(',')
				.Append(document.Pages)
				.AppendLine();
		}

		return new ExportResult(
			Format,
			"text/csv",
			csv.ToString().TrimEnd());
	}

	private static string Escape(string value)
	{
		var requiresQuotes =
			value.Contains(',') ||
			value.Contains('"') ||
			value.Contains('\n') ||
			value.Contains('\r');

		if (!requiresQuotes)
		{
			return value;
		}

		return $"\"{value.Replace("\"", "\"\"")}\"";
	}
}