using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using CaptureNET.Common;
using CaptureNET.PcapNG.Common;
using CaptureNET.PcapNG.Options;

namespace CaptureNET.PcapNG.Blocks
{
    public sealed class EnhancedPacketBlock : Block, IGenericPacket
    {
        private readonly uint _interfaceID;
        /// <summary>
        /// Interface ID (32 bits): an unsigned value that specifies the interface on which this packet was received or transmitted; the correct interface will be the one whose Interface Description Block (within the current Section of the file) is identified by the same number (see Section 4.2) of this field. The interface ID MUST be valid, which means that an matching interface description block MUST exist.
        /// </summary>
        public int InterfaceID => (int)_interfaceID;

        private readonly Timestamp _timestamp;
        /// <summary>
        /// Timestamp (High) and Timestamp (Low): upper 32 bits and lower 32 bits of a 64-bit timestamp. The timestamp is a single 64-bit unsigned integer that represents the number of units of time that have elapsed since 1970-01-01 00:00:00 UTC. The length of a unit of time is specified by the 'if_tsresol' option (see Figure 10) of the Interface Description Block referenced by this packet. Note that, unlike timestamps in the libpcap file format, timestamps in Enhanced Packet Blocks are not saved as two 32-bit values that represent the seconds and microseconds that have elapsed since 1970-01-01 00:00:00 UTC. Timestamps in Enhanced Packet Blocks are saved as two 32-bit words that represent the upper and lower 32 bits of a single 64-bit quantity.
        /// </summary>
        public Timestamp Timestamp => _timestamp;

        private readonly uint _capturedPacketLength;
        /// <summary>
        /// Captured Packet Length (32 bits): an unsigned value that indicates the number of octets captured from the packet (i.e. the length of the Packet Data field). It will be the minimum value among the Original Packet Length and the snapshot length for the interface (SnapLen, defined in Figure 10). The value of this field does not include the padding octets added at the end of the Packet Data field to align the Packet Data field to a 32-bit boundary.
        /// </summary>
        public uint CapturedPacketLength => _capturedPacketLength;

        private readonly uint _originalPacketLength;
        /// <summary>
        /// Original Packet Length (32 bits): an unsigned value that indicates the actual length of the packet when it was transmitted on the network. It can be different from the Captured Packet Length if the packet has been truncated by the capture process.
        /// </summary>
        public uint OriginalPacketLength => _originalPacketLength;

        /// <summary>
        /// Packet Data: the data coming from the network, including link-layer headers. The actual length of this field is Captured Packet Length plus the padding to a 32-bit boundary. The format of the link-layer headers depends on the LinkType field specified in the Interface Description Block (see Section 4.2) and it is specified in the entry for that format in [LINKTYPES].
        /// </summary>
        public ReadOnlySpan<byte> PacketData => _blockBody.Span.Slice(20, (int)_capturedPacketLength);

        private readonly EnhancedPacketOptions _options;
        /// <summary>
        /// Options: optionally, a list of options (formatted according to the rules defined in Section 3.5) can be present.
        /// </summary>
        public EnhancedPacketOptions Options => _options;

        /// <summary>
        /// An Enhanced Packet Block (EPB) is the standard container for storing the packets coming from the network. The Enhanced Packet Block is optional because packets can be stored either by means of this block or the Simple Packet Block, which can be used to speed up capture file generation; or a file may have no packets in it. The format of an Enhanced Packet Block is shown in Figure 11.
        /// </summary>
        public EnhancedPacketBlock(in uint interfaceID, in Timestamp timestamp, in uint originalPacketLength, in byte[] packetData, in EnhancedPacketOptions options)
            : base(BlockType.EnhancedPacket)
        {
            if (packetData == null)
                throw new ArgumentNullException($"{nameof(packetData)} cannot be null.");

            _interfaceID = interfaceID;
            _timestamp = timestamp;
            _capturedPacketLength = (uint)packetData.Length;
            _originalPacketLength = originalPacketLength;
            _options = options;

            _blockBody = new ReadOnlyMemory<byte>(BuildBlockBody(packetData));
            _blockTotalLength = (uint)AlignToBoundary(_blockBody.Length) + BlockOverhead;
        }

        public EnhancedPacketBlock(in BinaryReader binaryReader)
            : base(binaryReader, BlockType.EnhancedPacket)
        {
            if (binaryReader == null)
                throw new ArgumentNullException($"{nameof(binaryReader)} cannot be null.");

            ReadOnlySpan<byte> blockBody = _blockBody.Span;

            _interfaceID = BinaryPrimitives.ReadUInt32LittleEndian(blockBody.Slice(0, 4));
            _timestamp = new Timestamp(blockBody.Slice(4, 8));
            _capturedPacketLength = BinaryPrimitives.ReadUInt32LittleEndian(blockBody.Slice(12, 4));
            _originalPacketLength = BinaryPrimitives.ReadUInt32LittleEndian(blockBody.Slice(16, 4));

            int optionsOffset = 20 + AlignToBoundary((int)_capturedPacketLength);
            int optionsLength = blockBody.Length - optionsOffset;
            _options = optionsLength > 0 ? new EnhancedPacketOptions(blockBody[optionsOffset..]) : null;
        }

        private byte[] BuildBlockBody(in byte[] packetData)
        {
            List<byte> body = new List<byte>();
            body.AddRange(BitConverter.GetBytes(_interfaceID));
            body.AddRange(_timestamp.ToBytes());
            body.AddRange(BitConverter.GetBytes(_capturedPacketLength));
            body.AddRange(BitConverter.GetBytes(_originalPacketLength));
            body.AddRange(packetData);
            body.AddRange(AlignmentBytes(packetData.Length));
            if (_options != null)
                body.AddRange(_options.ToBytes());
            return body.ToArray();
        }
    }
}