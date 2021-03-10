using System;
using CaptureNET.PcapNG.Blocks;

namespace CaptureNET.PcapNG.Helpers
{
    public delegate void SectionHeaderBlockEventHandler(in SectionHeaderBlock sectionHeader);
    public delegate void InterfaceDescriptionBlockEventHandler(in InterfaceDescriptionBlock interfaceDescription);
    public delegate void EnhancedPacketBlockEventHandler(in EnhancedPacketBlock enhancedPacket);
    [Obsolete("Use EnhancedPacketBlock instead.", false)]
    public delegate void PacketBlockEventHandler(in PacketBlock packet);
    public delegate void SimplePacketEventHandler(in SimplePacketBlock simplePacket);
    public delegate void NameResolutionEventHandler(in NameResolutionBlock nameResolution);
    public delegate void InterfaceStatisticsEventHandler(in InterfaceStatisticsBlock interfaceStatistics);
    public delegate void BlockEventHandler(in Block block);
}
