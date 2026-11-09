using System.Text.Encodings.Web;
using System.Text.Json;
using Pos.Application.Views;

namespace Pos.Infrastructure.Export;

/// <summary>Выгрузка итогов в JSON.</summary>
public sealed class JsonGradeExporter : GradeExporterBase
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public override string Format => "json";
    protected override string Extension => "json";
    protected override string Render(IReadOnlyList<FinalGradeView> grades)
    {
        var rows = grades.Select(g => new
        {
            student = g.StudentName,
            final_grade = g.Grade,
            graded_criteria = g.GradedCriteria,
            total_criteria = g.TotalCriteria,
            policy = g.Policy,
        });
        return JsonSerializer.Serialize(rows, Options);
    }
}
