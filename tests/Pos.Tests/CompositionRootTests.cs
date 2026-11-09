using Pos.Application;
using Pos.ConsoleApp;

namespace Pos.Tests;

public class CompositionRootTests
{
    [Fact]
    public void Container_ResolvesWholeGraph_AndFacadeIsSingleton()
    {
        var container = CompositionRoot.Build(new StringReader(string.Empty), new StringWriter());

        Assert.Same(container.Resolve<PosFacade>(), container.Resolve<PosFacade>());
        Assert.NotNull(container.Resolve<ConsoleMenu>());
    }

    [Fact]
    public void DemoScenario_RunsEndToEnd_AndExports()
    {
        var output = new StringWriter();
        var container = CompositionRoot.Build(new StringReader(string.Empty), output);
        var directory = Path.Combine(Path.GetTempPath(), "pos-demo-" + Guid.NewGuid().ToString("N"));

        try
        {
            container.Resolve<DemoScenario>().Run(directory);

            var text = output.ToString();
            Assert.Contains("[ОТКЛОНЕНО] 12 баллов из 10", text);
            Assert.Contains("[ОТКЛОНЕНО] критерий не из рубрикатора", text);
            Assert.DoesNotContain("[?]", text);
            Assert.True(File.Exists(Path.Combine(directory, "final_grades.csv")));
            Assert.True(File.Exists(Path.Combine(directory, "final_grades.json")));

            var grades = container.Resolve<PosFacade>().GetFinalGrades();
            Assert.Equal(8.55m, grades.Single(g => g.StudentName == "Анна Смирнова").Grade);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public void Menu_ExitsOnEndOfInput()
    {
        var output = new StringWriter();
        var container = CompositionRoot.Build(new StringReader("2\n"), output);

        container.Resolve<ConsoleMenu>().Run();

        Assert.Contains("Студентов пока нет", output.ToString());
    }
}
