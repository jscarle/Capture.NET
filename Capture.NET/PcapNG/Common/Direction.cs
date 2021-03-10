using System;

namespace CaptureNET.PcapNG.Common
{
    [Flags]
    public enum Direction : uint
    {
        NotAvailable = 0x00000000,
        Inbound = 0x00000001,
        Outbound = 0x00000002
    }
}
