using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CaptureNET.PcapNG.Common;
using CaptureNET.PcapNG.Options.Helpers;

namespace CaptureNET.PcapNG.Options
{
    public sealed class PacketOptions : Options
    {
        /// <summary>
        /// The opt_comment option is a UTF-8 string containing human-readable comment text that is associated to the current block. Line separators SHOULD be a carriage-return + linefeed ('\r\n') or just linefeed ('\n'); either form may appear and be considered a line separator. The string is not zero-terminated.
        /// </summary>
        public List<string> Comments { get; }

        /// <summary>
        /// The epb_flags option is a 32-bit flags word containing link-layer information. A complete specification of the allowed flags can be found in Section 4.3.1.
        /// </summary>
        public PacketFlags? Flags { get; }

        /// <summary>
        /// The epb_hash option contains a hash of the packet. The first octet specifies the hashing algorithm, while the following octets contain the actual hash, whose size depends on the hashing algorithm, and hence from the value in the first octet. The hashing algorithm can be: 2s complement (algorithm octet = 0, size = XXX), XOR (algorithm octet = 1, size=XXX), CRC32 (algorithm octet = 2, size = 4), MD-5 (algorithm octet = 3, size = 16), SHA-1 (algorithm octet = 4, size = 20), Toeplitz (algorithm octet = 5, size = 4). The hash covers only the packet, not the header added by the capture driver: this gives the possibility to calculate it inside the network card. The hash allows easier comparison/merging of different capture files, and reliable data transfer between the data acquisition system and the capture library.
        /// </summary>
        public List<Hash> Hashes { get; }

        public PacketOptions(in List<string> comments = null, in PacketFlags? flags = null, in List<Hash> hashes = null)
        {
            Comments = comments;
            Flags = flags;
            Hashes = hashes;
        }

        public PacketOptions(in ReadOnlySpan<byte> bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");

            Comments = null;
            Flags = null;
            Hashes = null;

            foreach ((ushort key, byte[] value) in ReadOptions(bytes, out _))
            {
                switch (key)
                {
                    case (ushort)PacketOptionCodes.Comment:
                        Comments ??= new List<string>();
                        Comments.Add(Encoding.UTF8.GetString(value));
                        break;

                    case (ushort)PacketOptionCodes.Flags:
                        if (value.Length == 4)
                        {
                            uint packetBlockFlags = (BitConverter.ToUInt32(value, 0));
                            Flags = new PacketFlags(packetBlockFlags);
                        }
                        else
                            throw new ArgumentException($"Packet Flags is {value.Length} bytes instead of the expected 4 bytes.");
                        break;

                    case (ushort)PacketOptionCodes.Hash:
                        Hashes ??= new List<Hash>();
                        Hashes.Add(new Hash(value));
                        break;

                    case (ushort)PacketOptionCodes.EndOfOptions:
                        break;

                    default:
                        Debug.WriteLine($"Unknown Packet Options Code of {key}.");
                        break;
                }
            }
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();

            if (Comments != null)
            {
                foreach (string comment in Comments)
                {
                    byte[] commentValueBytes = Encoding.UTF8.GetBytes(comment);
                    if (commentValueBytes.Length <= UInt16.MaxValue)
                        bytes.AddRange(ConvertOptionFieldToBytes((ushort)PacketOptionCodes.Comment,
                            commentValueBytes));
                }
            }

            if (Flags.HasValue)
            {
                byte[] flagsValueBytes = BitConverter.GetBytes(Flags.Value.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)PacketOptionCodes.Flags, flagsValueBytes));
            }

            if (Hashes != null)
            {
                foreach (Hash hash in Hashes)
                {
                    byte[] hashValueBytes = hash.ToBytes();
                    if (hashValueBytes.Length <= UInt16.MaxValue)
                        bytes.AddRange(ConvertOptionFieldToBytes((ushort)PacketOptionCodes.Hash,
                            hashValueBytes));
                }
            }

            if (bytes.Count > 0)
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)PacketOptionCodes.EndOfOptions, Array.Empty<byte>()));

            return bytes.ToArray();
        }
    }
}