using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using CaptureNET.PcapNG.Common;
using CaptureNET.PcapNG.Options;

namespace CaptureNET.PcapNG.Blocks
{
    public sealed class InterfaceStatisticsBlock : Block
    {
        private readonly uint _interfaceID;
        /// <summary>
        /// Interface ID (32 bits): an unsigned value that specifies the interface on which this packet was received or transmitted; the correct interface will be the one whose Interface Description Block (within the current Section of the file) is identified by the same number (see Section 4.2) of this field. The interface ID MUST be valid, which means that an matching interface description block MUST exist.
        /// </summary>
        public uint InterfaceID => _interfaceID;

        private readonly Timestamp _timestamp;
        /// <summary>
        /// Timestamp (High) and Timestamp (Low): upper 32 bits and lower 32 bits of a 64-bit timestamp. The timestamp is a single 64-bit unsigned integer that represents the number of units of time that have elapsed since 1970-01-01 00:00:00 UTC. The length of a unit of time is specified by the 'if_tsresol' option (see Figure 10) of the Interface Description Block referenced by this packet. Note that, unlike timestamps in the libpcap file format, timestamps in Packet Blocks are not saved as two 32-bit values that represent the seconds and microseconds that have elapsed since 1970-01-01 00:00:00 UTC. Timestamps in Packet Blocks are saved as two 32-bit words that represent the upper and lower 32 bits of a single 64-bit quantity.
        /// </summary>
        public Timestamp Timestamp => _timestamp;

        private readonly InterfaceStatisticsOptions _options;
        /// <summary>
        /// Options: optionally, a list of options (formatted according to the rules defined in Section 3.5) can be present.
        /// </summary>
        public InterfaceStatisticsOptions Options => _options;

        public uint Seconds => _timestamp.Seconds;
        public uint Microseconds => _timestamp.Microseconds;

        /// <summary>
        /// An Packet Block (EPB) is the standard container for storing the packets coming from the network. The Packet Block is optional because packets can be stored either by means of this block or the Simple Packet Block, which can be used to speed up capture file generation; or a file may have no packets in it. The format of an Packet Block is shown in Figure 11.
        /// </summary>
        public InterfaceStatisticsBlock(in uint interfaceID, in Timestamp timestamp, in InterfaceStatisticsOptions options)
            : base(BlockType.InterfaceStatistics)
        {
            _interfaceID = interfaceID;
            _timestamp = timestamp;
            _options = options;

            _blockBody = new ReadOnlyMemory<byte>(BuildBlockBody());
            _blockTotalLength = (uint)AlignToBoundary(_blockBody.Length) + BlockOverhead;
        }

        public InterfaceStatisticsBlock(in BinaryReader binaryReader)
            : base(binaryReader, BlockType.InterfaceStatistics)
        {
            if (binaryReader == null)
                throw new ArgumentNullException($"{nameof(binaryReader)} cannot be null.");

            ReadOnlySpan<byte> blockBody = BlockBody.Span;

            _interfaceID = BinaryPrimitives.ReadUInt32LittleEndian(blockBody.Slice(0, 4));
            _timestamp = new Timestamp(blockBody.Slice(4, 8));

            int optionsOffset = 12;
            int optionsLength = blockBody.Length - optionsOffset;
            _options = optionsLength > 0 ? new InterfaceStatisticsOptions(blockBody[optionsOffset..]) : null;
        }

        private byte[] BuildBlockBody()
        {
            List<byte> body = new List<byte>();
            body.AddRange(BitConverter.GetBytes(_interfaceID));
            body.AddRange(_timestamp.ToBytes());
            if (_options != null)
                body.AddRange(_options.ToBytes());
            return body.ToArray();
        }
    }
}