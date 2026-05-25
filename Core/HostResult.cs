namespace NetworkScanner.Core;

public class HostResult
{
    public string IpAddress { get; set; } = string.Empty;
    public string HostName { get; set; } = "-";
    public string MacAddress { get; set; } = "-";
    public string Vendor { get; set; } = "-";
    public bool IsOnline { get; set; }
    public long? LatencyMs { get; set; }
}