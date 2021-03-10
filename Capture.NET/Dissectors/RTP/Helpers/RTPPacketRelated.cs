using CaptureNET.Dissectors.Ethernet;
using CaptureNET.Dissectors.IP.IPv4;
using CaptureNET.Dissectors.UDP;
using CaptureNET.PcapNG.Blocks;

namespace CaptureNET.Dissectors.RTP.Helpers
{
    public sealed class RTPPacketRelated
    {
        private readonly UDPDatagram _udpDatagram;
        public UDPDatagram UDPDatagram => _udpDatagram;

        public IPv4Packet IPv4Packet => _udpDatagram.Related.IPv4Packet;

        public EthernetFrame EthernetFrame => _udpDatagram.Related.EthernetFrame;

        public Block Block => _udpDatagram.Related.Block;

        public RTPPacketRelated(in UDPDatagram udpDatagram)
        {
            _udpDatagram = udpDatagram;
        }
    }
}
