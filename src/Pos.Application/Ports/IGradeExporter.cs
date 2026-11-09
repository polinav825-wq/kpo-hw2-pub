using Pos.Application.Views;

namespace Pos.Application.Ports;

/// <summary>Порт выгрузки итогов. Реализации (CSV, JSON) — в Infrastructure.</summary>
public interface IGradeExporter
{
    /// <summary>Код формата для выбора пользователем, например "csv".</summary>
    string Format { get; }

    /// <returns>Путь к созданному файлу.</returns>
    string Export(IReadOnlyList<FinalGradeView> grades, string directory);
}
