using System;
using System.Collections.Generic;
using System.IO;
using CaptureNET.Common;

namespace CaptureNET.Pcap
{
    public class PcapPacket : IGenericPacket
    {
        private protected const int PacketkOverhead = 16;

        private readonly uint _seconds;
        /// <summary>
        /// Timestamp (Seconds) and Timestamp (Microseconds or nanoseconds): seconds and fraction of a seconds values of a timestamp. The seconds value is a 32-bit unsigned integer that represents the number of seconds that have elapsed since 1970-01-01 00:00:00 UTC, and the microseconds or nanoseconds value represents the number of microseconds or nanoseconds that have elapsed since that seconds. Whether the value represents microseconds or nanoseconds is specified by the magic number in the File Header.
        /// </summary>
        public uint Seconds => _seconds;

        private readonly uint _microseconds;
        /// <summary>
        /// Timestamp (Seconds) and Timestamp (Microseconds or nanoseconds): seconds and fraction of a seconds values of a timestamp. The seconds value is a 32-bit unsigned integer that represents the number of seconds that have elapsed since 1970-01-01 00:00:00 UTC, and the microseconds or nanoseconds value represents the number of microseconds or nanoseconds that have elapsed since that seconds. Whether the value represents microseconds or nanoseconds is specified by the magic number in the File Header.
        /// </summary>
        public uint Microseconds => _microseconds;

        private readonly uint _capturedPacketLength;
        /// <summary>
        /// Captured Packet Length (32 bits): an unsigned value that indicates the number of octets captured from the packet(i.e.the length of the Packet Data field). It will be the minimum value among the Original Packet Length and the snapshot length for the interface (SnapLen, defined in Figure 1).
        /// </summary>
        public uint CapturedPacketLength => _capturedPacketLength;

        private readonly uint _originalPacketLength;
        /// <summary>
        /// Original Packet Length (32 bits): an unsigned value that indicates the actual length of the packet when it was transmitted on the network.It can be different from the Captured Packet Length if the packet has been truncated by the capture process.
        /// </summary>
        public uint OriginalPacketLength => _originalPacketLength;

        private protected ReadOnlyMemory<byte> _packetData;
        /// <summary>
        /// Packet Data: the data coming from the network, including link-layer headers.The actual length of this field is Captured Packet Length.The format of the link-layer headers depends on the LinkType field specified in the file header (see Figure 1) and it is specified in the entry for that format in [LINKTYPES].
        /// </summary>
        public ReadOnlySpan<byte> PacketData => _packetData.Span;

        private readonly long _positionInStream;
        public long PositionInStream => _positionInStream;

        public PcapPacket(in uint seconds, in uint microseconds, in byte[] packetData, in long positionInStream = 0)
        {
            if (packetData == null)
                throw new ArgumentNullException($"{nameof(packetData)} cannot be null.");

            _seconds = seconds;
            _microseconds = microseconds;
            _packetData = new ReadOnlyMemory<byte>(packetData);
            _capturedPacketLength = (uint)_packetData.Length;
            _originalPacketLength = (uint)_packetData.Length;

            _positionInStream = positionInStream;
        }

        public PcapPacket(in BinaryReader binaryReader, in bool nanosecondResolution = false)
        {
            _positionInStream = binaryReader.BaseStream.Position;

            _seconds = binaryReader.ReadUInt32();
            _microseconds = binaryReader.ReadUInt32();
            if (nanosecondResolution)
                _microseconds /= 1000;
            _capturedPacketLength = binaryReader.ReadUInt32();
            _originalPacketLength = binaryReader.ReadUInt32();

            _packetData = new ReadOnlyMemory<byte>(binaryReader.ReadBytes((int)CapturedPacketLength));
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>((int)CapturedPacketLength + PacketkOverhead);
            bytes.AddRange(BitConverter.GetBytes(Seconds));
            bytes.AddRange(BitConverter.GetBytes(Microseconds));
            bytes.AddRange(BitConverter.GetBytes(CapturedPacketLength));
            bytes.AddRange(BitConverter.GetBytes(OriginalPacketLength));
            bytes.AddRange(_packetData.ToArray());
            return bytes.ToArray();
        }
    }
}