using System;
using System.Buffers.Binary;
using System.Collections.Immutable;
using CaptureNET.Dissectors.RTP.Helpers;
using CaptureNET.Dissectors.UDP;

namespace CaptureNET.Dissectors.RTP
{
    public sealed class RTPPacket
    {
        private readonly byte _version;
        public byte Version => _version;

        private readonly bool _padding;
        public bool Padding => _padding;

        private readonly bool _extension;
        public bool Extension => _extension;

        private readonly bool _marker;
        public bool Marker => _marker;

        private readonly byte _payloadType;
        public byte PayloadType => _payloadType;

        private readonly ushort _sequenceNumber;
        public ushort SequenceNumber => _sequenceNumber;

        private readonly uint _timestamp;
        public uint Timestamp => _timestamp;

        private readonly uint _ssrc;
        public uint SSRC => _ssrc;

        private readonly ImmutableList<uint> _csrc = ImmutableList<uint>.Empty;
        public ImmutableList<uint> CSRC => _csrc;

        private readonly RTPPacketRelated _related;
        public RTPPacketRelated Related => _related;

        public RTPPacket(in UDPDatagram udpDatagram)
            : this(udpDatagram.Payload)
        {
            _related = new RTPPacketRelated(udpDatagram);
        }

        public RTPPacket(in ReadOnlySpan<byte> bytes)
        {
            _version = (byte)((bytes[0] & 0xC0) >> 6);
            _padding = (bytes[0] & 0x20) == 0x20;
            _extension = (bytes[0] & 0x10) == 0x10;
            byte csrcCount = (byte)(bytes[0] & 0x0F);
            _marker = (bytes[1] & 0x80) == 0x80;
            _payloadType = (byte)(bytes[1] & 0x7F);
            _sequenceNumber = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(2, 2));
            _timestamp = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(4, 4));
            _ssrc = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(8, 4));

            int offset = 12;
            if (csrcCount > 0)
                for (int csrcIndex = 0; csrcIndex < csrcCount; csrcIndex++)
                    _csrc = _csrc.Add(BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset + (csrcIndex * 4), 4)));
        }
    }
}
