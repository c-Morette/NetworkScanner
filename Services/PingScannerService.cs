using System.Net;
using System.Net.NetworkInformation;
using NetworkScanner.Core;

namespace NetworkScanner.Services;

public class PingScannerService
{
    private readonly MacAddressService _macAddressService = new();
    private readonly VendorLookupService _vendorLookupService = new();

    public async Task<List<HostResult>> ScanAsync(ScanOptions options)
    {
        var results = new List<HostResult>();
        var tasks = new List<Task<HostResult>>();

        for (int i = options.Start; i <= options.End; i++)
        {
            string ip = $"{options.BaseIp}.{i}";
            tasks.Add(PingAsync(ip, options.TimeoutMs));
        }

        var scanResults = await Task.WhenAll(tasks);

        results.AddRange(scanResults.Where(result => result.IsOnline));

        return results
            .OrderBy(result => IPAddress.Parse(result.IpAddress).GetAddressBytes(), new ByteArrayComparer())
            .ToList();
    }

    private async Task<HostResult> PingAsync(string ipAddress, int timeoutMs)
    {
        using var ping = new Ping();
    
        try
        {
            var reply = await ping.SendPingAsync(ipAddress, timeoutMs);
    
            if (reply.Status == IPStatus.Success)
            {
                string macAddress = await _macAddressService.GetMacAddressAsync(ipAddress);
                string vendor = _vendorLookupService.GetVendor(macAddress);
    
                return new HostResult
                {
                    IpAddress = ipAddress,
                    HostName = await TryGetHostNameAsync(ipAddress),
                    MacAddress = macAddress,
                    Vendor = vendor,
                    IsOnline = true,
                    LatencyMs = reply.RoundtripTime
                };
            }
        }
        catch
        {
            // Ignora IPs que falharem.
        }
    
        return new HostResult
        {
            IpAddress = ipAddress,
            HostName = "-",
            MacAddress = "-",
            Vendor = "-",
            IsOnline = false,
            LatencyMs = 0
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