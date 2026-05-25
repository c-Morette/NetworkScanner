namespace NetworkScanner.Core;
public class ScanOptions
{
    public string BaseIp { get; set; } = string.Empty;
    public int Start { get; set; } = 1;
    public int End { get; set; } = 254;
    public int TimeoutMs { get; set; } = 1500;
}