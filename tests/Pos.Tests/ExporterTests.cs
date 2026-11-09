using System.Text.Json;
using Pos.Application.Views;
using Pos.Infrastructure.Export;

namespace Pos.Tests;

public sealed class ExporterTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "pos-tests-" + Guid.NewGuid().ToString("N"));

    private static readonly FinalGradeView[] Grades =
    {
        new(Guid.NewGuid(), "Анна Смирнова", 8.55m, 3, 3, "Итоговая"),
        new(Guid.NewGuid(), "Борис; Кузнецов", 5.1m, 2, 3, "Итоговая"),
    };

    [Fact]
    public void Csv_WritesHeaderAndRows_EscapingSeparator()
    {
        var path = new CsvGradeExporter().Export(Grades, directory);

        var lines = File.ReadAllLines(path);
        Assert.EndsWith(".csv", path);
        Assert.Equal("student;final_grade;graded_criteria;total_criteria;policy", lines[0]);
        Assert.Equal("Анна Смирнова;8.55;3;3;Итоговая", lines[1]);
        Assert.Equal("\"Борис; Кузнецов\";5.1;2;3;Итоговая", lines[2]);
    }

    [Fact]
    public void Json_WritesParsableArray()
    {
        var path = new JsonGradeExporter().Export(Grades, directory);

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var rows = document.RootElement;
        Assert.Equal(2, rows.GetArrayLength());
        Assert.Equal("Анна Смирнова", rows[0].GetProperty("student").GetString());
        Assert.Equal(8.55m, rows[0].GetProperty("final_grade").GetDecimal());
    }

    [Fact]
    public void Export_EmptyDirectory_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CsvGradeExporter().Export(Grades, " "));
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
