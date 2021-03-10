using System;
using System.Collections.Generic;
using System.IO;
using CaptureNET.PcapNG.Options;

namespace CaptureNET.PcapNG.Blocks
{
    public sealed class NameResolutionBlock : Block
    {
        private readonly NameResolutionRecords _records;
        public NameResolutionRecords Records => _records;

        private readonly NameResolutionOptions _options;
        /// <summary>
        /// optional fields. Optional fields can be used to insert some information that may be useful when reading data, but that is not
        /// really needed for packet processing. Therefore, each tool can either read the content of the optional fields (if any),
        /// or skip some of them or even all at once.
        /// </summary>
        public NameResolutionOptions Options => _options;

        /// <summary>
        /// The Name Resolution Block is used to support the correlation of numeric addresses (present in the captured packets) and their
        /// corresponding canonical names and it is optional. Having the literal names saved in the file, this prevents the need of a name
        /// resolution in a delayed time, when the association between names and addresses can be different from the one in use at capture time.
        /// Moreover, the Name Resolution Block avoids the need of issuing a lot of DNS requests every time the trace capture is opened,
        /// and allows to have name resolution also when reading the capture with a machine not connected to the network.
        /// A Name Resolution Block is normally placed at the beginning of the file, but no assumptions can be taken about its position.
        /// Name Resolution Blocks can be added in a second time by tools that process the file, like network analyzers.
        /// </summary>
        public NameResolutionBlock(in NameResolutionRecords records, in NameResolutionOptions options)
            : base(BlockType.NameResolution)
        {
            _records = records;
            _options = options;

            _blockBody = new ReadOnlyMemory<byte>(BuildBlockBody());
            _blockTotalLength = (uint)AlignToBoundary(_blockBody.Length) + BlockOverhead;
        }

        public NameResolutionBlock(in BinaryReader binaryReader)
            : base(binaryReader, BlockType.NameResolution)
        {
            if (binaryReader == null)
                throw new ArgumentNullException($"{nameof(binaryReader)} cannot be null.");

            ReadOnlySpan<byte> blockBody = _blockBody.Span;

            _records = new NameResolutionRecords(blockBody, out int optionsOffset);

            int optionsLength = blockBody.Length - optionsOffset;
            _options = optionsLength > 0 ? new NameResolutionOptions(blockBody[optionsOffset..]) : null;
        }

        private byte[] BuildBlockBody()
        {
            List<byte> body = new List<byte>();
            body.AddRange(_records.ToBytes());
            if (_options != null)
                body.AddRange(_options.ToBytes());
            return body.ToArray();
        }
    }
}