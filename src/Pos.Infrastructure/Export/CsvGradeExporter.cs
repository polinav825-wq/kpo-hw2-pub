using System.Globalization;
using System.Text;
using Pos.Application.Views;

namespace Pos.Infrastructure.Export;

/// <summary>Выгрузка итогов в CSV (разделитель «;», с BOM — чтобы Excel правильно открыл кириллицу).</summary>
public sealed class CsvGradeExporter : GradeExporterBase
{
    private const char Separator = ';';
    public override string Format => "csv";
    protected override string Extension => "csv";
    protected override Encoding FileEncoding => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
    protected override string Render(IReadOnlyList<FinalGradeView> grades)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(Separator, "student", "final_grade", "graded_criteria", "total_criteria", "policy"));
        foreach (var grade in grades)
        {
            builder.AppendLine(string.Join(
                Separator,
                Escape(grade.StudentName),
                grade.Grade.ToString(CultureInfo.InvariantCulture),
                grade.GradedCriteria.ToString(CultureInfo.InvariantCulture),
                grade.TotalCriteria.ToString(CultureInfo.InvariantCulture),
                Escape(grade.Policy)));
        }
        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (value.IndexOfAny(new[] { Separator, '"', '\n', '\r' }) < 0)
        {
            return value;
        }
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
