using System.Text;
using Pos.Application.Ports;
using Pos.Application.Views;

namespace Pos.Infrastructure.Export;

/// <summary>
/// Template Method (поведенческий GoF): общий каркас выгрузки итогов —
/// проверить аргументы → подготовить папку → сформировать содержимое → записать файл.
/// Наследники меняют только формат: расширение файла и само содержимое.
/// </summary>
public abstract class GradeExporterBase : IGradeExporter
{
    public abstract string Format { get; }
    protected abstract string Extension { get; }
    protected virtual Encoding FileEncoding => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Шаблонный метод: порядок шагов фиксирован и не переопределяется.</summary>
    public string Export(IReadOnlyList<FinalGradeView> grades, string directory)
    {
        ArgumentNullException.ThrowIfNull(grades);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Не указана папка для выгрузки.", nameof(directory));
        }

        Directory.CreateDirectory(directory);
        var content = Render(grades);
        var path = Path.Combine(directory, $"final_grades.{Extension}");
        File.WriteAllText(path, content, FileEncoding);
        return path;
    }
    /// <summary>Шаг, который у каждого формата свой.</summary>
    protected abstract string Render(IReadOnlyList<FinalGradeView> grades);
}
