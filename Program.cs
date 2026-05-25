using NetworkScanner.Core;
using NetworkScanner.Services;
using NetworkScanner.UI;
using Spectre.Console;

var scanner = new PingScannerService();

var cliDefaults = new ScanOptions
{
    MaxConcurrentProbes = ParseConcurrentArg(args)
};

bool keepRunning = true;

while (keepRunning)
{
    ConsoleRenderer.ShowHeader();

    var options = ConsoleRenderer.AskScanOptions(cliDefaults);

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

static int ParseConcurrentArg(string[] args)
{
    for (int i = 0; i < args.Length; i++)
    {
        string arg = args[i];

        if (arg is "--concurrent" or "-c")
        {
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out int next) && next > 0)
                return next;
        }
        else if (arg.StartsWith("--concurrent=", StringComparison.Ordinal))
        {
            if (int.TryParse(arg["--concurrent=".Length..], out int inline) && inline > 0)
                return inline;
        }
        else if (arg.StartsWith("-c=", StringComparison.Ordinal))
        {
            if (int.TryParse(arg["-c=".Length..], out int inline) && inline > 0)
                return inline;
        }
    }

    return 0;
}