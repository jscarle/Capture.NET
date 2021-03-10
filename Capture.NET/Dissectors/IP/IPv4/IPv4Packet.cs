using System;
using System.Buffers.Binary;
using System.Net;
using CaptureNET.Common.Helpers;
using CaptureNET.Dissectors.Ethernet;
using CaptureNET.Dissectors.IP.IPv4.Helpers;

namespace CaptureNET.Dissectors.IP.IPv4
{
    public sealed class IPv4Packet
    {
        private readonly byte _version;
        public byte Version => _version;

        private readonly byte _ihl;
        public byte IHL => _ihl;

        private readonly byte _dscp;
        public byte DSCP => _dscp;

        private readonly ECN _ecn;
        public ECN ECN => _ecn;

        private readonly ushort _totalLength;
        public ushort TotalLength => _totalLength;

        private readonly ushort _id;
        public ushort ID => _id;

        private readonly IPv4Flags _flags;
        public IPv4Flags Flags => _flags;

        private readonly ushort _fragmentOffset;
        public ushort FragmentOffset => _fragmentOffset;

        private readonly byte _ttl;
        public byte TTL => _ttl;

        private readonly IPProtocol _protocol;
        public IPProtocol Protocol => _protocol;

        private readonly ushort _checksum;
        public ushort Checksum => _checksum;

        private readonly IPAddress _sourceIpAddress;
        public IPAddress SourceIPAddress => _sourceIpAddress;

        private readonly IPAddress _destinationIpAddress;
        public IPAddress DestinationIPAddress => _destinationIpAddress;

        private readonly byte[] _options;
        public byte[] Options => _options;

        private readonly int _payloadStart;
        private readonly int _payloadLength;
        public ReadOnlySpan<byte> Payload => _related.EthernetFrame.Payload.Slice(_payloadStart, _payloadLength);

        private readonly IPv4PacketRelated _related;
        public IPv4PacketRelated Related => _related;

        public IPv4Packet(in EthernetFrame ethernetFrame)
            : this(ethernetFrame.Payload)
        {
            _related = new IPv4PacketRelated(ethernetFrame);
        }

        public IPv4Packet(in ReadOnlySpan<byte> bytes)
        {
            _version = (byte)(((uint)bytes[0] & 0xF0) >> 4);
            _ihl = (byte)((uint)bytes[0] & 0x0F);
            _dscp = (byte)(((uint)bytes[1] & 0xFC) >> 2);
            _ecn = (ECN)(((uint)bytes[1] & 0x03) >> 2);
            _totalLength = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(2, 2));
            _id = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(4, 2));
            _flags = new IPv4Flags((byte)(((uint)bytes[6] & 0xE0) >> 5));
            _fragmentOffset = (ushort)((uint)BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(6, 2)) << 3);
            _ttl = bytes[8];
#if DEBUG
            if (!Enum.IsDefined(typeof(IPProtocol), bytes[9]))
                throw new NotImplementedException($"Unknown IP Protocol of 0x{bytes[9]:x2}.");
#endif
            _protocol = (IPProtocol)bytes[9];
            _checksum = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(10, 2));
            _sourceIpAddress = new IPAddress(bytes.Slice(12, 4).ToArray());
            _destinationIpAddress = new IPAddress(bytes.Slice(16, 4).ToArray());

            int offset = 20;
            _options = IHL > 5 ? bytes.Forward(ref offset, (IHL - 5) * 4).ToArray() : Array.Empty<byte>();

            _payloadStart = offset;
            _payloadLength = bytes.Length - offset;
        }
    }
}
