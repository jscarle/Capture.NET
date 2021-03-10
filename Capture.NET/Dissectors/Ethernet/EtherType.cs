namespace CaptureNET.Dissectors.Ethernet
{
    public enum EtherType : ushort
    {
        ARP = 0x0806,
        IEEE8021Q = 0x8100,
        IEEE8022SNAP = 0x05DA,
        IEEE8022LLC = 0x05DB,
        IEEE8023Ethernet = 0x05DC,
        IPv4 = 0x0800,
        IPv6 = 0x86DD,
        LLDP = 0x88CC
    }
}
