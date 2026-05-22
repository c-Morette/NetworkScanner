using NetworkScanner.Core;
using Spectre.Console;

namespace NetworkScanner.UI;

public static class ConsoleRenderer
{
    public static void ShowHeader()
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("Network Scanner")
                .Centered()
                .Color(Color.Yellow));

        AnsiConsole.MarkupLine("[grey]Simple .NET network scanner using Spectre.Console[/]");
        AnsiConsole.WriteLine();
    }

    public static ScanOptions AskScanOptions()
    {
        string baseIp = AnsiConsole.Ask<string>(
            "Enter the base IP, example [yellow]192.168.1[/]:");

        int start = AnsiConsole.Ask<int>("Start range:", 1);
        int end = AnsiConsole.Ask<int>("End range:", 254);

        return new ScanOptions
        {
            BaseIp = baseIp,
            Start = start,
            End = end,
            TimeoutMs = 1000
        };
    }

    public static void ShowResults(List<HostResult> hosts)
    {
        AnsiConsole.WriteLine();

        if (hosts.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No active hosts found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[yellow]Active Hosts[/]")
            .AddColumn("[grey]IP Address[/]")
            .AddColumn("[grey]Host Name[/]")
            .AddColumn("[grey]MAC Address[/]")
            .AddColumn("[grey]Vendor[/]")
            .AddColumn("[grey]Status[/]")
            .AddColumn("[grey]Latency[/]");

        foreach (var host in hosts)
        {
           table.AddRow(
                $"[white]{host.IpAddress}[/]",
                $"[grey]{host.HostName}[/]",
                $"[blue]{host.MacAddress}[/]",
                $"[yellow]{host.Vendor}[/]",
                "[green]Online[/]",
                $"[yellow]{host.LatencyMs} ms[/]");
        }

        AnsiConsole.Write(table);
    }
}