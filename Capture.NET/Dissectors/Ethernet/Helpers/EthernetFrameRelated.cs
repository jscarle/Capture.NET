using CaptureNET.PcapNG.Blocks;

namespace CaptureNET.Dissectors.Ethernet.Helpers
{
    public sealed class EthernetFrameRelated
    {
        private readonly Block _block;
        public Block Block => _block;

        public EthernetFrameRelated(in Block block)
        {
            _block = block;
        }
    }
}
