using System.Net;
using System.Runtime.InteropServices;

namespace NetworkScanner.Services;

public class ArpTableService
{
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const int MIB_IPNET_TYPE_DYNAMIC = 3;
    private const int MIB_IPNET_TYPE_STATIC = 4;

    public Dictionary<string, string> GetArpTable()
    {
        var table = new Dictionary<string, string>(StringComparer.Ordinal);

        int size = 0;
        int firstCall = GetIpNetTable(IntPtr.Zero, ref size, false);

        if (firstCall != ERROR_INSUFFICIENT_BUFFER || size == 0)
            return table;

        IntPtr buffer = Marshal.AllocHGlobal(size);

        try
        {
            int secondCall = GetIpNetTable(buffer, ref size, false);

            if (secondCall != 0)
                return table;

            int entries = Marshal.ReadInt32(buffer);
            IntPtr rowPtr = IntPtr.Add(buffer, sizeof(int));
            int rowSize = Marshal.SizeOf<MIB_IPNETROW>();

            for (int i = 0; i < entries; i++)
            {
                var row = Marshal.PtrToStructure<MIB_IPNETROW>(rowPtr);

                bool isUsableType = row.dwType == MIB_IPNET_TYPE_DYNAMIC
                    || row.dwType == MIB_IPNET_TYPE_STATIC;

                if (isUsableType && row.dwPhysAddrLen == 6)
                {
                    string ip = new IPAddress(BitConverter.GetBytes(row.dwAddr)).ToString();
                    string mac = $"{row.mac0:X2}{row.mac1:X2}{row.mac2:X2}{row.mac3:X2}{row.mac4:X2}{row.mac5:X2}";

                    if (IsUsableMac(mac))
                        table[ip] = mac;
                }

                rowPtr = IntPtr.Add(rowPtr, rowSize);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return table;
    }

    private static bool IsUsableMac(string mac)
    {
        return mac != "000000000000" && mac != "FFFFFFFFFFFF";
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetIpNetTable(IntPtr pIpNetTable, ref int pdwSize, bool bOrder);

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_IPNETROW
    {
        public int dwIndex;
        public int dwPhysAddrLen;
        public byte mac0;
        public byte mac1;
        public byte mac2;
        public byte mac3;
        public byte mac4;
        public byte mac5;
        public byte mac6;
        public byte mac7;
        public uint dwAddr;
        public int dwType;
    }
}
