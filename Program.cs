using NetworkScanner.Services;
using NetworkScanner.UI;
using Spectre.Console;

var scanner = new PingScannerService();

bool keepRunning = true;

while (keepRunning)
{
    ConsoleRenderer.ShowHeader();

    var options = ConsoleRenderer.AskScanOptions();

    var results = await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("yellow"))
        .StartAsync("Scanning network...", async ctx =>
        {
            return await scanner.ScanAsync(options);
        });

    ConsoleRenderer.ShowResults(results);

    AnsiConsole.WriteLine();

    string nextAction = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What do you want to do next?")
            .AddChoices(
                "Scan again",
                "Exit"));

    if (nextAction == "Exit")
    {
        keepRunning = false;
    }
}

AnsiConsole.MarkupLine("[grey]Application closed.[/]");