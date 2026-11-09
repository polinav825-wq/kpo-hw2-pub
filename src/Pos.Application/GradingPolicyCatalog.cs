using Pos.Domain.Grading;

namespace Pos.Application;

/// <summary>Набор правил итога, между которыми можно переключаться. Первое — правило по умолчанию.</summary>
public sealed class GradingPolicyCatalog
{
    private readonly IReadOnlyList<IGradingPolicy> policies;

    public GradingPolicyCatalog(params IGradingPolicy[] policies)
    {
        if (policies is null || policies.Length == 0)
        {
            throw new ArgumentException("Нужно хотя бы одно правило итога.", nameof(policies));
        }

        this.policies = policies;
    }

    public IGradingPolicy Default => policies[0];

    public IReadOnlyList<IGradingPolicy> All => policies;

    public IGradingPolicy Get(string code) =>
        policies.FirstOrDefault(p => string.Equals(p.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? throw new PosOperationException($"Неизвестное правило итога «{code}».");
}
