using System.Globalization;

namespace Pos.ConsoleApp;

/// <summary>Чтение и разбор ввода пользователя. Принимает и «8.5», и «8,5».</summary>
public sealed class ConsoleInput
{
    private readonly TextReader input;
    private readonly TextWriter output;

    public ConsoleInput(TextReader input, TextWriter output)
    {
        this.input = input ?? throw new ArgumentNullException(nameof(input));
        this.output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>Строка от пользователя; null — ввод закончился.</summary>
    public string? Ask(string prompt)
    {
        output.Write(prompt);
        return input.ReadLine();
    }

    public string AskText(string prompt) =>
        Ask(prompt) ?? throw new EndOfInputException();

    public decimal AskDecimal(string prompt)
    {
        while (true)
        {
            var text = AskText(prompt).Trim().Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            output.WriteLine("  Нужно число, например 7 или 8.5.");
        }
    }

    /// <summary>Выбор элемента списка по номеру (1..N). Пустая строка — отмена.</summary>
    public T? Choose<T>(string title, IReadOnlyList<T> items, Func<T, string> describe)
        where T : class
    {
        if (items.Count == 0)
        {
            output.WriteLine("  Список пуст.");
            return null;
        }

        output.WriteLine(title);
        for (var i = 0; i < items.Count; i++)
        {
            output.WriteLine($"  {i + 1}. {describe(items[i])}");
        }

        while (true)
        {
            var text = AskText("Номер (Enter — отмена): ").Trim();
            if (text.Length == 0)
            {
                return null;
            }

            if (int.TryParse(text, out var number) && number >= 1 && number <= items.Count)
            {
                return items[number - 1];
            }

            output.WriteLine($"  Введите число от 1 до {items.Count}.");
        }
    }
}

/// <summary>Ввод закончился (Ctrl+D или конец файла) — меню завершает работу.</summary>
public sealed class EndOfInputException : Exception
{
}
