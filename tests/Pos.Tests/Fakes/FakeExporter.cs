using Pos.Application.Ports;
using Pos.Application.Views;

namespace Pos.Tests.Fakes;

/// <summary>Подставной адаптер выгрузки: ничего не пишет на диск, только запоминает данные.</summary>
public sealed class FakeExporter : IGradeExporter
{
    public IReadOnlyList<FinalGradeView>? LastExported { get; private set; }
    public string Format => "fake";
    public string Export(IReadOnlyList<FinalGradeView> grades, string directory)
    {
        LastExported = grades;
        return $"{directory}/fake";
    }
}
