using CaptureNET.Dissectors.Ethernet;
using CaptureNET.Dissectors.IP.IPv4;
using CaptureNET.PcapNG.Blocks;

namespace CaptureNET.Dissectors.UDP.Helpers
{
    public sealed class UDPDatagramRelated
    {
        private readonly IPv4Packet _ipv4Packet;
        public IPv4Packet IPv4Packet => _ipv4Packet;

        public EthernetFrame EthernetFrame => _ipv4Packet.Related.EthernetFrame;

        public Block Block => _ipv4Packet.Related.Block;

        public UDPDatagramRelated(in IPv4Packet ipv4Packet)
        {
            _ipv4Packet = ipv4Packet;
        }
    }
}
