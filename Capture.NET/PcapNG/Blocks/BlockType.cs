namespace CaptureNET.PcapNG.Blocks
{
    public enum BlockType : uint
    {
        SectionHeader = 0x0A0D0D0A,
        InterfaceDescription = 0x00000001,
        Packet = 0x00000002,
        SimplePacket = 0x00000003,
        NameResolution = 0x00000004,
        InterfaceStatistics = 0x00000005,
        EnhancedPacket = 0x00000006,
        CustomCanCopy = 0x00000BAD,
        CustomShouldNotCopy = 0x40000BAD
    }
}