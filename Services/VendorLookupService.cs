namespace NetworkScanner.Services;

public class VendorLookupService
{
    private readonly Dictionary<string, string> _vendors = new();

    public VendorLookupService()
    {
        LoadVendors();
    }

    public string GetVendor(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress) || macAddress == "-")
            return "-";

        string normalized = NormalizeMac(macAddress);

        if (normalized.Length < 6)
            return "-";

        string oui = normalized[..6];

        return _vendors.TryGetValue(oui, out string? vendor)
            ? vendor
            : "-";
    }

    private void LoadVendors()
    {
        var assembly = typeof(VendorLookupService).Assembly;

        string? resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("Data.manuf.txt"));

        if (resourceName is null)
        {
            return;
        }

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            return;
        }

        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            string? rawLine = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            string line = rawLine.Trim();

            if (line.StartsWith("#"))
                continue;

            string[] parts = line.Split(
                [' ', '\t'],
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                continue;

            string prefix = parts[0]
                .Replace(":", "")
                .Replace("-", "")
                .Replace(".", "")
                .ToUpperInvariant();

            if (prefix.Length < 6)
                continue;

            string vendor = parts.Length >= 3
                ? string.Join(" ", parts.Skip(2)).Trim()
                : parts[1].Trim();

            if (string.IsNullOrWhiteSpace(vendor))
                vendor = parts[1].Trim();

            string key = prefix[..6];

            _vendors.TryAdd(key, vendor);
        }
    }

    private static string NormalizeMac(string macAddress)
    {
        return macAddress
            .Replace(":", "")
            .Replace("-", "")
            .Replace(".", "")
            .Trim()
            .ToUpperInvariant();
    }
}