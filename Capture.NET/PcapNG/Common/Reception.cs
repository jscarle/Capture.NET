using System;

namespace CaptureNET.PcapNG.Common
{
    [Flags]
    public enum Reception : uint
    {
        NotAvailable = 0x00000000,
        Unicast = 0x00000004,
        Multicast = 0x00000008,
        Broadcast = 0x0000000C,
        Promiscuous = 0x00000010,
    }
}
