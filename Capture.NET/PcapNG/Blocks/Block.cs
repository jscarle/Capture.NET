using System;
using System.Collections.Generic;
using System.IO;

namespace CaptureNET.PcapNG.Blocks
{
    public abstract class Block
    {
        private const int AlignmentBoundary = 4;
        private protected const int BlockOverhead = 12;

        private readonly BlockType _blockType;
        /// <summary>
        /// Block Type (32 bits): a unique unsigned value that identifies the block. Values whose Most Significant Bit (MSB) is equal to 1 are reserved for local use. They can be used to make extensions to the file format to save private data to the file. The list of currently defined types can be found in Section 11.1.
        /// </summary>
        public BlockType BlockType => _blockType;

        private protected uint _blockTotalLength;
        /// <summary>
        /// Block Total Length (32 bits): an unsigned value giving the total size of this block, in octets. For instance, the length of a block that does not have a body is 12 octets: 4 octets for the Block Type, 4 octets for the initial Block Total Length and 4 octets for the trailing Block Total Length. This value MUST be a multiple of 4.
        /// </summary>
        public uint BlockTotalLength => _blockTotalLength;

        private protected ReadOnlyMemory<byte> _blockBody;
        /// <summary>
        /// Block Body: content of the block.
        /// </summary>
        public ReadOnlyMemory<byte> BlockBody => _blockBody;

        private readonly long _positionInStream;
        public long PositionInStream => _positionInStream;

        private protected Block(in BlockType blockType)
        {
            _blockType = blockType;
        }

        /// <summary>
        /// A capture file is organized in blocks, that are appended one to another to form the file. All the blocks share a common format, which is shown in Figure 1.
        /// </summary>
        private protected Block(in BlockType blockType, in byte[] blockBody, in long positionInStream = 0)
        {
            if (blockBody == null)
                throw new ArgumentNullException($"{nameof(blockBody)} cannot be null.");

            _blockType = blockType;
            _blockTotalLength = (uint)_blockBody.Length + BlockOverhead;
            _blockBody = new ReadOnlyMemory<byte>(blockBody);

            _positionInStream = positionInStream;
        }

        private protected Block(in BinaryReader binaryReader, in BlockType blockType)
        {
            _blockType = blockType;
            _positionInStream = binaryReader.BaseStream.Position - 4;

            uint forwardTotalLength = binaryReader.ReadUInt32();
            if (forwardTotalLength < BlockOverhead)
                throw new InvalidDataException($"Insufficient Block Total Length of {forwardTotalLength} bytes.");

            _blockBody = new ReadOnlyMemory<byte>(binaryReader.ReadBytes((int)forwardTotalLength - BlockOverhead));

            uint backwardsTotalLength = binaryReader.ReadUInt32();
            if (backwardsTotalLength < BlockOverhead)
                throw new InvalidDataException($"Insufficient Block Total Length of {forwardTotalLength} bytes.");

            if (forwardTotalLength != backwardsTotalLength)
                throw new InvalidDataException($"Forward Block Total Length of {forwardTotalLength} bytes is different than the Backwards Block Total Length of {backwardsTotalLength} bytes.");

            _blockTotalLength = forwardTotalLength;
        }

        private protected static int AlignToBoundary(in int offset)
        {
            return offset + ((AlignmentBoundary - offset % AlignmentBoundary) % AlignmentBoundary);
        }

        private protected static byte[] AlignmentBytes(in int unalignedLength)
        {
            return new byte[(AlignmentBoundary - unalignedLength % AlignmentBoundary) % AlignmentBoundary];
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>((int)_blockTotalLength);
            bytes.AddRange(BitConverter.GetBytes((uint)_blockType));
            bytes.AddRange(BitConverter.GetBytes(_blockTotalLength));
            bytes.AddRange(_blockBody.ToArray());
            bytes.AddRange(BitConverter.GetBytes(_blockTotalLength));
            return bytes.ToArray();
        }
    }
}