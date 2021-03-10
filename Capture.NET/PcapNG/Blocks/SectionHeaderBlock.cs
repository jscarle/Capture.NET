using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using CaptureNET.PcapNG.Options;

namespace CaptureNET.PcapNG.Blocks
{
    public sealed class SectionHeaderBlock : Block
    {
        private readonly MagicNumbers _byteOrderMagic;
        /// <summary>
        /// Byte-Order Magic (32 bits): an unsigned magic number, whose value is the hexadecimal number 0x1A2B3C4D. This number can be used to distinguish sections that have been saved on little-endian machines from the ones saved on big-endian machines, and to heuristically identify pcapng files.
        /// </summary>
        public MagicNumbers ByteOrderMagic => _byteOrderMagic;

        private readonly ushort _majorVersion;
        /// <summary>
        /// Major Version (16 bits): an unsigned value, giving the number of the current major version of the format. The value for the current version of the format is 1. This value should change if the format changes in such a way that code that reads the new format could not read the old format (i.e., code to read both formats would have to check the version number and use different code paths for the two formats) and code that reads the old format could not read the new format. Note that adding a new block type or a new option is NOT such a change.
        /// </summary>
        public ushort MajorVersion => _majorVersion;

        private readonly ushort _minorVersion;
        /// <summary>
        /// Minor Version (16 bits): an unsigned value, giving the number of the current minor version of the format. The value is for the current version of the format is 0. This value should change if the format changes in such a way that code that reads the new format could read the old format without checking the version number but code that reads the old format could not read all files in the new format. Note that adding a new block type or a new option is NOT such a change.
        /// </summary>
        public ushort MinorVersion => _minorVersion;

        private readonly long _sectionLength;
        /// <summary>
        /// Section Length (64 bits): a signed value specifying the length in octets of the following section, excluding the Section Header Block itself. This field can be used to skip the section, for faster navigation inside large files. If the Section Length is -1 (0xFFFFFFFFFFFFFFFF), this means that the size of the section is not specified, and the only way to skip the section is to parse the blocks that it contains. Please note that if this field is valid (i.e. not negative), its value is always a multiple of 4, as all the blocks are aligned to and padded to 32-bit (4 octet) boundaries. Also, special care should be taken in accessing this field: since the alignment of all the blocks in the file is 32-bits, this field is not guaranteed to be aligned to a 64-bit boundary. This could be a problem on 64-bit processors.
        /// </summary>
        public long SectionLength => _sectionLength;

        private readonly SectionHeaderOptions _options;
        /// <summary>
        /// optional fields. Optional fields can be used to insert some information that may be useful when reading data, but that is not
        /// really needed for packet processing. Therefore, each tool can either read the content of the optional fields (if any),
        /// or skip some of them or even all at once.
        /// </summary>
        public SectionHeaderOptions Options => _options;

        public bool ReverseByteOrder => ByteOrderMagic == MagicNumbers.Swapped;

        /// <summary>
        /// The Section Header Block (SHB) is mandatory. It identifies the beginning of a section of the capture file. The Section Header Block does not contain data but it rather identifies a list of blocks (interfaces, packets) that are logically correlated. Its format is shown in Figure 9.
        /// </summary>
        public SectionHeaderBlock(in SectionHeaderOptions options)
            : base(BlockType.SectionHeader)
        {
            _byteOrderMagic = MagicNumbers.Identical;
            _majorVersion = 1;
            _minorVersion = 0;
            _sectionLength = -1;
            _options = options;

            _blockBody = new ReadOnlyMemory<byte>(BuildBlockBody());
            _blockTotalLength = (uint)AlignToBoundary(_blockBody.Length) + BlockOverhead;
        }

        public SectionHeaderBlock(in BinaryReader binaryReader)
            : base(binaryReader, BlockType.SectionHeader)
        {
            if (binaryReader == null)
                throw new ArgumentNullException($"{nameof(binaryReader)} cannot be null.");

            ReadOnlySpan<byte> blockBody = _blockBody.Span;

            uint byteOrderMagic = BinaryPrimitives.ReadUInt32LittleEndian(blockBody.Slice(0, 4));
#if DEBUG
            if (!Enum.IsDefined(typeof(MagicNumbers), byteOrderMagic))
                throw new InvalidDataException($"Unrecognized Magic Number of 0x{byteOrderMagic:x8}.");
#endif
            _byteOrderMagic = (MagicNumbers)byteOrderMagic;
            _majorVersion = BinaryPrimitives.ReadUInt16LittleEndian(blockBody.Slice(4, 2));
            _minorVersion = BinaryPrimitives.ReadUInt16LittleEndian(blockBody.Slice(6, 2));
            _sectionLength = BinaryPrimitives.ReadInt64LittleEndian(blockBody.Slice(8, 8));

            int optionsOffset = 16;
            int optionsLength = blockBody.Length - optionsOffset;
            _options = optionsLength > 0 ? new SectionHeaderOptions(blockBody[optionsOffset..]) : null;
        }

        private byte[] BuildBlockBody()
        {
            List<byte> body = new List<byte>();
            body.AddRange(BitConverter.GetBytes(((uint)_byteOrderMagic)));
            body.AddRange(BitConverter.GetBytes(_majorVersion));
            body.AddRange(BitConverter.GetBytes(_minorVersion));
            body.AddRange(BitConverter.GetBytes(_sectionLength));
            if (_options != null)
                body.AddRange(_options.ToBytes());
            return body.ToArray();
        }
    }
}