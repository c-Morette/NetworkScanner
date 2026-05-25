using System.Net;
using System.Net.NetworkInformation;
using NetworkScanner.Core;

namespace NetworkScanner.Services;

public class PingScannerService
{
    private const int ArpSettleDelayMs = 500;
    private const int PingAttempts = 3;
    private const int PingRetryDelayMs = 1000;

    private readonly MacAddressService _macAddressService = new();
    private readonly VendorLookupService _vendorLookupService = new();
    private readonly ArpTableService _arpTableService = new();

    public async Task<List<HostResult>> ScanAsync(ScanOptions options)
    {
        // Limita a concorrência para não saturar o WiFi com rajadas que o
        // chip do celular pode descartar pensando ser flood / ataque.
        int concurrentProbes = options.MaxConcurrentProbes > 0 ? options.MaxConcurrentProbes : 32;
        using var probeSemaphore = new SemaphoreSlim(concurrentProbes);

        var probeTasks = new List<Task<PingProbeResult>>();

        for (int i = options.Start; i <= options.End; i++)
        {
            string ip = $"{options.BaseIp}.{i}";
            probeTasks.Add(ProbeWithThrottleAsync(ip, options.TimeoutMs, probeSemaphore));
        }

        var probes = await Task.WhenAll(probeTasks);

        await Task.Delay(ArpSettleDelayMs);

        var arpTable = _arpTableService.GetArpTable();

        var resultsByIp = new Dictionary<string, HostResult>(StringComparer.Ordinal);

        var pingedHostTasks = probes
            .Where(probe => probe.IsAlive)
            .Select(probe => BuildPingedHostAsync(probe, arpTable))
            .ToList();

        var pingedResults = await Task.WhenAll(pingedHostTasks);

        foreach (var result in pingedResults)
            resultsByIp[result.IpAddress] = result;

        var arpOnlyTasks = arpTable
            .Where(entry => !resultsByIp.ContainsKey(entry.Key) && IsInRange(entry.Key, options))
            .Select(entry => BuildArpOnlyHostAsync(entry.Key, entry.Value))
            .ToList();

        var arpOnlyResults = await Task.WhenAll(arpOnlyTasks);

        foreach (var result in arpOnlyResults)
            resultsByIp[result.IpAddress] = result;

        return resultsByIp.Values
            .OrderBy(result => IPAddress.Parse(result.IpAddress).GetAddressBytes(), new ByteArrayComparer())
            .ToList();
    }

    private async Task<HostResult> BuildPingedHostAsync(PingProbeResult probe, Dictionary<string, string> arpTable)
    {
        string macAddress = arpTable.TryGetValue(probe.IpAddress, out string? arpMac)
            ? arpMac
            : await _macAddressService.GetMacAddressAsync(probe.IpAddress);

        return new HostResult
        {
            IpAddress = probe.IpAddress,
            HostName = await TryGetHostNameAsync(probe.IpAddress),
            MacAddress = macAddress,
            Vendor = _vendorLookupService.GetVendor(macAddress),
            IsOnline = true,
            LatencyMs = probe.LatencyMs
        };
    }

    private async Task<HostResult> BuildArpOnlyHostAsync(string ip, string mac)
    {
        return new HostResult
        {
            IpAddress = ip,
            HostName = await TryGetHostNameAsync(ip),
            MacAddress = mac,
            Vendor = _vendorLookupService.GetVendor(mac),
            IsOnline = true,
            LatencyMs = null
        };
    }

    private static async Task<PingProbeResult> ProbeWithThrottleAsync(string ip, int timeoutMs, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();

        try
        {
            return await TryPingAsync(ip, timeoutMs);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static async Task<PingProbeResult> TryPingAsync(string ip, int timeoutMs)
    {
        using var ping = new Ping();

        for (int attempt = 0; attempt < PingAttempts; attempt++)
        {
            if (attempt > 0)
                await Task.Delay(PingRetryDelayMs);

            try
            {
                var reply = await ping.SendPingAsync(ip, timeoutMs);

                if (reply.Status == IPStatus.Success)
                {
                    return new PingProbeResult
                    {
                        IpAddress = ip,
                        IsAlive = true,
                        LatencyMs = reply.RoundtripTime
                    };
                }
            }
            catch
            {
                // Tenta o próximo attempt.
            }
        }

        return new PingProbeResult
        {
            IpAddress = ip,
            IsAlive = false,
            LatencyMs = null
        };
    }

    private static async Task<string> TryGetHostNameAsync(string ipAddress)
    {
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(ipAddress);

            if (string.IsNullOrWhiteSpace(hostEntry.HostName))
                return "-";

            return CleanHostName(hostEntry.HostName);
        }
        catch
        {
            return "-";
        }
    }

    private static string CleanHostName(string hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName))
            return "-";

        string cleanName = hostName.Trim();

        int dotIndex = cleanName.IndexOf('.');

        if (dotIndex > 0)
            cleanName = cleanName[..dotIndex];

        return cleanName;
    }

    private static bool IsInRange(string ip, ScanOptions options)
    {
        string prefix = options.BaseIp + ".";

        if (!ip.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        string lastOctet = ip[prefix.Length..];

        if (!int.TryParse(lastOctet, out int octet))
            return false;

        return octet >= options.Start && octet <= options.End;
    }

    private sealed class PingProbeResult
    {
        public string IpAddress { get; init; } = string.Empty;
        public bool IsAlive { get; init; }
        public long? LatencyMs { get; init; }
    }
}

public class ByteArrayComparer : IComparer<byte[]>
{
    public int Compare(byte[]? x, byte[]? y)
    {
        if (x is null || y is null)
            return 0;

        for (int i = 0; i < Math.Min(x.Length, y.Length); i++)
        {
            int result = x[i].CompareTo(y[i]);

            if (result != 0)
                return result;
        }
        return x.Length.CompareTo(y.Length);
    }
}
