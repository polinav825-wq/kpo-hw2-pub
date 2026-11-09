namespace Pos.Application.Views;

// DTO, которыми Application общается с внешним миром (консоль, экспорт).
// Доменные объекты наружу не отдаются.

public sealed record StudentView(Guid Id, string Name);

public sealed record CriterionView(Guid Id, string Name, decimal MaxScore, decimal Weight);

public sealed record CriterionDraft(string Name, decimal MaxScore, decimal Weight);

public sealed record EvaluationView(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid CriterionId,
    string CriterionName,
    decimal Score,
    decimal MaxScore,
    DateTimeOffset GradedAt,
    string? Comment);

public sealed record FinalGradeView(
    Guid StudentId,
    string StudentName,
    decimal Grade,
    int GradedCriteria,
    int TotalCriteria,
    string Policy);

public sealed record GradingPolicyView(string Code, string Title, bool IsCurrent);
