using System.Net;
using ArpLookup;

namespace NetworkScanner.Services;

public class MacAddressService
{
    public async Task<string> GetMacAddressAsync(string ipAddress)
    {
        try
        {
            if (!IPAddress.TryParse(ipAddress, out var parsedIp))
                return "-";

            var macAddress = await Arp.LookupAsync(parsedIp);

            if (macAddress is null)
                return "-";

            return macAddress.ToString();
        }
        catch
        {
            return "-";
        }
    }
}