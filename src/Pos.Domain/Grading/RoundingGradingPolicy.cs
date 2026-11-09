using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

/// <summary>
/// Decorator (структурный GoF): навешивает округление на любое правило итога.
/// Правило расчёта и правило округления меняются независимо — без копий каждой стратегии «с округлением».
/// </summary>
public sealed class RoundingGradingPolicy : IGradingPolicy
{
    private readonly IGradingPolicy inner;
    private readonly int decimals;

    public RoundingGradingPolicy(IGradingPolicy inner, int decimals)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        if (decimals < 0 || decimals > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(decimals), decimals, "Допустимо от 0 до 4 знаков.");
        }

        this.decimals = decimals;
    }

    public string Code => inner.Code;

    public string Title => decimals == 0
        ? $"{inner.Title}, округление до целого"
        : $"{inner.Title}, округление до {decimals} зн.";

    public decimal Calculate(Rubric rubric, IReadOnlyCollection<Evaluation> evaluations) =>
        Math.Round(inner.Calculate(rubric, evaluations), decimals, MidpointRounding.AwayFromZero);
}
