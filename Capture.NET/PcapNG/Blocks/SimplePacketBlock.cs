using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using CaptureNET.Common;

namespace CaptureNET.PcapNG.Blocks
{
    public sealed class SimplePacketBlock : Block, IGenericPacket
    {
        private readonly uint _originalPacketLength;
        /// <summary>
        /// Original Packet Length (32 bits): an unsigned value that indicates the actual length of the packet when it was transmitted on the network. It can be different from the Captured Packet Length if the packet has been truncated by the capture process.
        /// </summary>
        public uint OriginalPacketLength => _originalPacketLength;

        /// <summary>
        /// Packet Data: the data coming from the network, including link-layer headers. The actual length of this field is Captured Packet Length plus the padding to a 32-bit boundary. The format of the link-layer headers depends on the LinkType field specified in the Interface Description Block (see Section 4.2) and it is specified in the entry for that format in [LINKTYPES].
        /// </summary>
        public ReadOnlySpan<byte> PacketData => _blockBody.Span[4..(int)_blockTotalLength];

        /// <summary>
        /// An Simple Packet Block (EPB) is the standard container for storing the packets coming from the network. The Simple Packet Block is optional because packets can be stored either by means of this block or the Simple Packet Block, which can be used to speed up capture file generation; or a file may have no packets in it. The format of an Simple Packet Block is shown in Figure 11.
        /// </summary>
        public SimplePacketBlock(in uint originalPacketLength, in byte[] packetData)
            : base(BlockType.SimplePacket)
        {
            if (packetData == null)
                throw new ArgumentNullException($"{nameof(packetData)} cannot be null.");

            _originalPacketLength = originalPacketLength;

            _blockBody = new ReadOnlyMemory<byte>(BuildBlockBody(packetData));
            _blockTotalLength = (uint)AlignToBoundary(_blockBody.Length) + BlockOverhead;
        }

        public SimplePacketBlock(in BinaryReader binaryReader)
            : base(binaryReader, BlockType.SimplePacket)
        {
            if (binaryReader == null)
                throw new ArgumentNullException($"{nameof(binaryReader)} cannot be null.");

            ReadOnlySpan<byte> blockBody = _blockBody.Span;

            _originalPacketLength = BinaryPrimitives.ReadUInt32LittleEndian(blockBody.Slice(0, 4));
        }

        private byte[] BuildBlockBody(in byte[] packetData)
        {
            List<byte> body = new List<byte>();
            body.AddRange(BitConverter.GetBytes(_originalPacketLength));
            body.AddRange(packetData);
            body.AddRange(AlignmentBytes(packetData.Length));
            return body.ToArray();
        }
    }
}