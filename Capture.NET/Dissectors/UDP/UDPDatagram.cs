using System;
using System.Buffers.Binary;
using CaptureNET.Dissectors.IP.IPv4;
using CaptureNET.Dissectors.UDP.Helpers;

namespace CaptureNET.Dissectors.UDP
{
    public sealed class UDPDatagram
    {
        private readonly ushort _sourcePort;
        public ushort SourcePort => _sourcePort;

        private readonly ushort _destinationPort;
        public ushort DestinationPort => _destinationPort;

        private readonly ushort _length;
        public ushort Length => _length;

        private readonly ushort _checksum;
        public ushort Checksum => _checksum;

        private readonly UDPDatagramRelated _related;
        public UDPDatagramRelated Related => _related;

        private readonly int _payloadStart;
        private readonly int _payloadLength;
        public ReadOnlySpan<byte> Payload => _related.IPv4Packet.Payload.Slice(_payloadStart, _payloadLength);

        public UDPDatagram(in IPv4Packet ipv4Packet)
            : this(ipv4Packet.Payload)
        {
            _related = new UDPDatagramRelated(ipv4Packet);
        }

        public UDPDatagram(in ReadOnlySpan<byte> bytes)
        {
            _sourcePort = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(0, 2));
            _destinationPort = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(2, 2));
            _length = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(4, 2));
            _checksum = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(6, 2));

            int offset = 8;
            _payloadStart = offset;
            _payloadLength = bytes.Length - offset;
        }
    }
}
