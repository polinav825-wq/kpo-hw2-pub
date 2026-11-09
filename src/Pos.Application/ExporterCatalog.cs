using Pos.Application.Ports;

namespace Pos.Application;

/// <summary>Набор доступных форматов выгрузки (собирается в composition root).</summary>
public sealed class ExporterCatalog
{
    private readonly IReadOnlyList<IGradeExporter> exporters;

    public ExporterCatalog(params IGradeExporter[] exporters)
    {
        if (exporters is null || exporters.Length == 0)
        {
            throw new ArgumentException("Нужен хотя бы один формат выгрузки.", nameof(exporters));
        }

        this.exporters = exporters;
    }

    public IReadOnlyList<string> Formats => exporters.Select(e => e.Format).ToList();

    public IGradeExporter Get(string format) =>
        exporters.FirstOrDefault(e => string.Equals(e.Format, format?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? throw new PosOperationException(
            $"Неизвестный формат «{format}». Доступны: {string.Join(", ", Formats)}.");
}
