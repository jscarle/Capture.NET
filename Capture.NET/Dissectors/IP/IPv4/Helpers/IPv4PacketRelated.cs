using CaptureNET.Dissectors.Ethernet;
using CaptureNET.PcapNG.Blocks;

namespace CaptureNET.Dissectors.IP.IPv4.Helpers
{
    public sealed class IPv4PacketRelated
    {
        private readonly EthernetFrame _ethernetFrame;
        public EthernetFrame EthernetFrame => _ethernetFrame;

        public Block Block => _ethernetFrame.Related.Block;

        public IPv4PacketRelated(in EthernetFrame ethernetFrame)
        {
            _ethernetFrame = ethernetFrame;
        }
    }
}
