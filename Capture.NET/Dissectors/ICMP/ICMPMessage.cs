using System;
using System.Buffers.Binary;
using CaptureNET.Dissectors.IP.IPv4;
using CaptureNET.Dissectors.ICMP.Helpers;

namespace CaptureNET.Dissectors.ICMP
{
    public sealed class ICMPMessage
    {
        private readonly byte _icmpType;
        public byte ICMPType => _icmpType;

        private readonly byte _icmpCode;
        public byte ICMPCode => _icmpCode;

        private readonly ushort _checksum;
        public ushort Checksum => _checksum;

        public ReadOnlySpan<byte> Header => _related.IPv4Packet.Payload.Slice(_payloadStart, _payloadLength);

        private readonly ICMPMessageRelated _related;
        public ICMPMessageRelated Related => _related;

        private readonly int _payloadStart;
        private readonly int _payloadLength;
        public ReadOnlySpan<byte> Payload => _related.IPv4Packet.Payload.Slice(_payloadStart, _payloadLength);

        public ICMPMessage(in IPv4Packet ipv4Packet)
            : this(ipv4Packet.Payload)
        {
            _related = new ICMPMessageRelated(ipv4Packet);
        }

        public ICMPMessage(in ReadOnlySpan<byte> bytes)
        {
            _icmpType = bytes[0];
            _icmpCode = bytes[1];
            _checksum = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(2, 2));

            int offset = 8;
            _payloadStart = offset;
            _payloadLength = bytes.Length - offset;
        }
    }
}