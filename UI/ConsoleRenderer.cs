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

    public static ScanOptions AskScanOptions(ScanOptions defaults)
    {
        string baseIp = AnsiConsole.Ask<string>(
            "Enter the base IP, example [yellow]192.168.1[/]:");

        int start = AnsiConsole.Ask<int>("Start range:", defaults.Start);
        int end = AnsiConsole.Ask<int>("End range:", defaults.End);

        int concurrent = defaults.MaxConcurrentProbes > 0
            ? defaults.MaxConcurrentProbes
            : AnsiConsole.Ask<int>("Parallel IP checks:", 32);

        return new ScanOptions
        {
            BaseIp = baseIp,
            Start = start,
            End = end,
            TimeoutMs = defaults.TimeoutMs,
            MaxConcurrentProbes = concurrent
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
            string latencyMarkup = host.LatencyMs.HasValue
                ? $"[yellow]{host.LatencyMs} ms[/]"
                : "[grey]ARP[/]";

            table.AddRow(
                $"[white]{host.IpAddress}[/]",
                $"[grey]{host.HostName}[/]",
                $"[blue]{host.MacAddress}[/]",
                $"[yellow]{host.Vendor}[/]",
                "[green]Online[/]",
                latencyMarkup);
        }

        AnsiConsole.Write(table);
    }
}