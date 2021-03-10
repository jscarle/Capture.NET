namespace CaptureNET.PcapNG.Common
{
    public readonly struct PacketFlags
    {
        private readonly Direction _direction;
        public Direction Direction => _direction;

        private readonly Reception _reception;
        public Reception Reception => _reception;

        private readonly byte _fcsLength;
        public byte FCSLength => _fcsLength;

        private readonly LinkLayerError _linkLayerError;
        public LinkLayerError LinkLayerError => _linkLayerError;

        private readonly uint _value;
        public uint Value => _value;

        public PacketFlags(in uint flags)
        {
            _direction = (Direction)(flags & 0x00000003);
            _reception = (Reception)(flags & 0x0000001C);
            _fcsLength = (byte)(flags & 0x000001E0);
            _linkLayerError = (LinkLayerError)(flags & 0xFFFF0000);
            _value = flags;
        }
    }
}