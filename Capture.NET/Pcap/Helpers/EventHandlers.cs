namespace CaptureNET.Pcap.Helpers
{
    public delegate void HeaderEventHandler(in PcapHeader header);
    public delegate void PacketEventHandler(in PcapPacket packet);
}
