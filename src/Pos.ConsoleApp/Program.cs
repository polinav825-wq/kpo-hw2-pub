using Pos.ConsoleApp;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var container = CompositionRoot.Build(Console.In, Console.Out);

if (args.Contains("--demo"))
{
    container.Resolve<DemoScenario>().Run();
}
else
{
    container.Resolve<ConsoleMenu>().Run();
}
