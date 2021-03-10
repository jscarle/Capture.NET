using System;
using System.Collections.Generic;
using System.IO;
using CaptureNET.Common;

namespace CaptureNET.Pcap
{
    public class PcapHeader
    {
        private readonly MagicNumbers _magicNumber;
        /// <summary>
        /// Magic Number (32 bits): an unsigned magic number, whose value is either the hexadecimal number 0xA1B2C3D4 or the hexadecimal number 0xA1B23C4D. If the value is 0xA1B2C3D4, time stamps in Packet Records(see Figure 2) are in seconds and microseconds; if it is 0xA1B23C4D, time stamps in Packet Records are in seconds and nanoseconds. These numbers can be used to distinguish sessions that have been saved on little-endian machines from the ones saved on big-endian machines, and to heuristically identify pcap files.
        /// </summary>
        public MagicNumbers MagicNumber => _magicNumber;

        private readonly ushort _majorVersion;
        /// <summary>
        /// Major Version (16 bits): an unsigned value, giving the number of the current major version of the format.The value for the current version of the format is 2. This value should change if the format changes in such a way that code that reads the new format could not read the old format(i.e., code to read both formats would have to check the version number and use different code paths for the two formats) and code that reads the old format could not read the new format.
        /// </summary>
        public ushort MajorVersion => _majorVersion;

        private readonly ushort _minorVersion;
        /// <summary>
        /// Minor Version (16 bits): an unsigned value, giving the number of the current minor version of the format.The value is for the current version of the format is 4. This value should change if the format changes in such a way that code that reads the new format could read the old format without checking the version number but code that reads the old format could not read all files in the new format.
        /// </summary>
        public ushort MinorVersion => _minorVersion;

        private readonly uint _reserved1;
        /// <summary>
        /// Reserved1 (32 bits): not used - SHOULD be filled with 0 by pcap file writers, and MUST be ignored by pcap file readers.This value was documented by some older implementations as "gmt to local correction". Some older pcap file writers stored non-zero values in this field.
        /// </summary>
        public uint Reserved1 => _reserved1;

        private readonly uint _reserved2;
        /// <summary>
        /// Reserved2 (32 bits): not used - SHOULD be filled with 0 by pcap file writers, and MUST be ignored by pcap file readers.This value was documented by some older implementations as "accuracy of timestamps". Some older pcap file writers stored non-zero values in this field.
        /// </summary>
        public uint Reserved2 => _reserved2;

        private readonly uint _snapLength;
        /// <summary>
        /// SnapLen (32 bits): an unsigned value indicating the maximum number of octets captured from each packet.The portion of each packet that exceeds this value will not be stored in the file. This value MUST NOT be zero; if no limit was specified, the value should be a number greater than or equal to the largest packet length in the file.
        /// </summary>
        public uint SnapLength => _snapLength;

        private readonly LinkTypes _linkType;
        /// <summary>
        /// network: link-layer header type, specifying the type of headers at the beginning of the packet (e.g. 1 for Ethernet,
        /// see tcpdump.org's link-layer header types page for details); this can be various types such as 802.11, 802.11 with various
        /// radio information, PPP, Token Ring, FDDI, etc.
        /// </summary>
        public LinkTypes LinkType => _linkType;

        public bool ReverseByteOrder => MagicNumber == MagicNumbers.MicrosecondSwapped || MagicNumber == MagicNumbers.NanosecondSwapped;
        public bool NanosecondResolution => MagicNumber == MagicNumbers.NanosecondIdentical || MagicNumber == MagicNumbers.NanosecondSwapped;

        public PcapHeader(in uint snapLength, in LinkTypes linkType, in bool nanosecondResolution)
        {
            _magicNumber = nanosecondResolution ? MagicNumbers.NanosecondIdentical : MagicNumbers.MicrosecondIdentical;
            _majorVersion = 2;
            _minorVersion = 4;
            _reserved1 = 0;
            _reserved2 = 0;
            _snapLength = snapLength;
            _linkType = linkType;
        }

        public PcapHeader(in BinaryReader binaryReader)
        {
            if (binaryReader == null)
                throw new ArgumentNullException($"{nameof(binaryReader)} cannot be null.");

            uint detectedMagicNumber = binaryReader.ReadUInt32();
#if DEBUG
            if (!Enum.IsDefined(typeof(MagicNumbers), detectedMagicNumber))
                throw new NotImplementedException($"Unrecognized Magic Number of {detectedMagicNumber:x8}.");
#endif
            _magicNumber = (MagicNumbers)detectedMagicNumber;

            _majorVersion = binaryReader.ReadUInt16();
            _minorVersion = binaryReader.ReadUInt16();
            _reserved1 = binaryReader.ReadUInt32();
            _reserved2 = binaryReader.ReadUInt32();
            _snapLength = binaryReader.ReadUInt32();
            uint linkType = binaryReader.ReadUInt32();
#if DEBUG
            if (!Enum.IsDefined(typeof(LinkTypes), (ushort)linkType))
                throw new NotImplementedException($"Invalid Link Type of {linkType}.");
#endif
            _linkType = (LinkTypes)linkType;

        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes((uint)_magicNumber));
            bytes.AddRange(BitConverter.GetBytes(_majorVersion));
            bytes.AddRange(BitConverter.GetBytes(_minorVersion));
            bytes.AddRange(BitConverter.GetBytes(_reserved1));
            bytes.AddRange(BitConverter.GetBytes(_reserved2));
            bytes.AddRange(BitConverter.GetBytes(_snapLength));
            bytes.AddRange(BitConverter.GetBytes((ushort)_linkType));
            return bytes.ToArray();
        }
    }
}