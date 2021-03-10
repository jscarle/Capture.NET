namespace CaptureNET.Dissectors.IP
{
    public enum IPProtocol : byte
    {
        ICMP = 0x01,
        IGMP = 0x02,
        TCP = 0x06,
        UDP = 0x11,
        IPv6 = 0x29,
        ESP = 0x32,
        AH = 0x33
    }
}
