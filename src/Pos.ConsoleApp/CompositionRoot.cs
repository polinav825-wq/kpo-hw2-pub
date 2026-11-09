using Pos.Application;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.DependencyInjection;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence;

namespace Pos.ConsoleApp;

/// <summary>
/// Composition root: единственное место, где выбираются конкретные реализации портов
/// и правила итога. Всё остальное зависит только от абстракций.
/// </summary>
public static class CompositionRoot
{
    public static DIContainer Build(TextReader input, TextWriter output)
    {
        var container = new DIContainer();

        container.RegisterInstance<TextReader>(input);
        container.RegisterInstance<TextWriter>(output);
        container.RegisterInstance<TimeProvider>(TimeProvider.System);

        // Адаптеры портов-репозиториев.
        container.RegisterSingleton<IStudentRepository, InMemoryStudentRepository>();
        container.RegisterSingleton<IRubricRepository, InMemoryRubricRepository>();
        container.RegisterSingleton<IEvaluationRepository, InMemoryEvaluationRepository>();

        // Правила итога (Strategy), каждое обёрнуто в округление (Decorator).
        container.RegisterInstance(new GradingPolicyCatalog(
            new RoundingGradingPolicy(new WeightedGradingPolicy(), decimals: 2),
            new RoundingGradingPolicy(new EvaluatedOnlyGradingPolicy(), decimals: 2)));

        // Форматы выгрузки (Template Method).
        container.RegisterInstance(new ExporterCatalog(new CsvGradeExporter(), new JsonGradeExporter()));

        container.RegisterSingleton<PosFacade, PosFacade>();

        return container;
    }
}
