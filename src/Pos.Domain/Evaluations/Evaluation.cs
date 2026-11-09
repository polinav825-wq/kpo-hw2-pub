using Pos.Domain.Common;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

/// <summary>
/// Оценка (Entity): факт «этому студенту по этому критерию поставили такой балл».
/// Создаётся только через EvaluationFactory; исправить можно лишь в пределах того же критерия.
/// </summary>
public sealed class Evaluation
{
    public const int MaxCommentLength = 500;

    internal Evaluation(
        EvaluationId id,
        StudentId studentId,
        CriterionId criterionId,
        Score score,
        DateTimeOffset gradedAt,
        string? comment)
    {
        Id = id;
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score ?? throw new ArgumentNullException(nameof(score));
        GradedAt = gradedAt;
        Comment = NormalizeComment(comment);
    }

    public EvaluationId Id { get; }

    public StudentId StudentId { get; }

    public CriterionId CriterionId { get; }

    public Score Score { get; private set; }

    public DateTimeOffset GradedAt { get; private set; }

    public string? Comment { get; private set; }

    /// <summary>Исправить балл. Максимум критерия сохраняется, диапазон проверяется заново.</summary>
    public void Correct(decimal newValue, string? comment, DateTimeOffset correctedAt)
    {
        var newScore = Score.WithValue(newValue);
        var newComment = NormalizeComment(comment) ?? Comment;

        Score = newScore;
        Comment = newComment;
        GradedAt = correctedAt;
    }

    private static string? NormalizeComment(string? comment)
    {
        var normalized = comment?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length > MaxCommentLength)
        {
            throw new DomainException($"Комментарий длиннее {MaxCommentLength} символов.");
        }

        return normalized;
    }
}
