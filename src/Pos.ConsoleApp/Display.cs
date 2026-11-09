using System.Globalization;
using Pos.Application.Views;

namespace Pos.ConsoleApp;

/// <summary>Единое форматирование представлений для вывода в консоль.</summary>
public static class Display
{
    public static string Number(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    public static string Of(StudentView student) => student.Name;

    public static string Of(CriterionView criterion) =>
        $"{criterion.Name} (макс. {Number(criterion.MaxScore)}, вес {Number(criterion.Weight)})";

    public static string Of(EvaluationView evaluation)
    {
        var comment = evaluation.Comment is null ? string.Empty : $" — «{evaluation.Comment}»";
        return $"{evaluation.StudentName} | {evaluation.CriterionName}: " +
               $"{Number(evaluation.Score)}/{Number(evaluation.MaxScore)}{comment}";
    }

    public static string Of(FinalGradeView grade) =>
        $"{grade.StudentName,-20} {Number(grade.Grade),6}  " +
        $"(оценено {grade.GradedCriteria} из {grade.TotalCriteria})";
}
