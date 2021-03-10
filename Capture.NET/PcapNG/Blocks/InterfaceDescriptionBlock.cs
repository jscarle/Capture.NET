using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using CaptureNET.Common;
using CaptureNET.PcapNG.Options;

namespace CaptureNET.PcapNG.Blocks
{
    public sealed class InterfaceDescriptionBlock : Block
    {
        private readonly LinkTypes _linkType;
        /// <summary>
        /// LinkType (16 bits): an unsigned value that defines the link layer type of this interface. The list of Standardized Link Layer Type codes is available in [LINKTYPES].
        /// </summary>
        public LinkTypes LinkType => _linkType;

        private readonly uint _snapLength;
        /// <summary>
        /// SnapLen (32 bits): an unsigned value indicating the maximum number of octets captured from each packet. The portion of each packet that exceeds this value will not be stored in the file. A value of zero indicates no limit.
        /// </summary>
        public uint SnapLength => _snapLength;

        private readonly InterfaceDescriptionOptions _options;
        /// <summary>
        /// Options: optionally, a list of options (formatted according to the rules defined in Section 3.5) can be present.
        /// </summary>
        public InterfaceDescriptionOptions Options => _options;

        /// <summary>
        /// An Interface Description Block (IDB) is the container for information describing an interface on which packet data is captured.
        /// </summary>
        public InterfaceDescriptionBlock(in LinkTypes linkType, in uint snapLength, in InterfaceDescriptionOptions options)
            : base(BlockType.InterfaceDescription)
        {
            _linkType = linkType;
            _snapLength = snapLength;
            _options = options;

            _blockBody = new ReadOnlyMemory<byte>(BuildBlockBody());
            _blockTotalLength = (uint)AlignToBoundary(_blockBody.Length) + BlockOverhead;
        }

        public InterfaceDescriptionBlock(in BinaryReader binaryReader)
            : base(binaryReader, BlockType.InterfaceDescription)
        {
            if (binaryReader == null)
                throw new ArgumentNullException($"{nameof(binaryReader)} cannot be null.");

            ReadOnlySpan<byte> blockBody = _blockBody.Span;

            ushort linkType = BinaryPrimitives.ReadUInt16LittleEndian(blockBody.Slice(0, 2));
#if DEBUG
            if (!Enum.IsDefined(typeof(LinkTypes), linkType))
                throw new NotImplementedException($"Unknown Link Type of 0x{linkType:x4}.");
#endif
            _linkType = (LinkTypes)linkType;
            // Reserved UInt16 Skipped
            _snapLength = BinaryPrimitives.ReadUInt32LittleEndian(blockBody.Slice(4, 4));

            int optionsOffset = 8;
            int optionsLength = blockBody.Length - optionsOffset;
            _options = optionsLength > 0 ? new InterfaceDescriptionOptions(blockBody[optionsOffset..]) : null;
        }

        public byte[] BuildBlockBody()
        {
            List<byte> body = new List<byte>();
            body.AddRange(BitConverter.GetBytes(((ushort)_linkType)));
            body.AddRange(new byte[] { 0, 0 }); // Reserved
            body.AddRange(BitConverter.GetBytes(_snapLength));
            if (_options != null)
                body.AddRange(_options.ToBytes());
            return body.ToArray();
        }
    }
}