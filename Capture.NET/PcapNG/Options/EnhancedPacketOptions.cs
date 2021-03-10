using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CaptureNET.PcapNG.Common;
using CaptureNET.PcapNG.Options.Helpers;

namespace CaptureNET.PcapNG.Options
{
    public sealed class EnhancedPacketOptions : Options
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

        /// <summary>
        /// The epb_dropcount option is a 64-bit unsigned integer value specifying the number of packets lost (by the interface and the operating system) between this packet and the preceding one for the same interface or, for the first packet for an interface, between this packet and the start of the capture process.
        /// </summary>
        public ulong? DropCount { get; }

        /// <summary>
        /// The epb_packetid option is a 64-bit unsigned integer that uniquely identifies the packet. If the same packet is seen by multiple interfaces and there is a way for the capture application to correlate them, the same epb_packetid value must be used. An example could be a router that captures packets on all its interfaces in both directions. When a packet hits interface A on ingress, an EPB entry gets created, TTL gets decremented, and right before it egresses on interface B another EPB entry gets created in the trace file. In this case, two packets are in the capture file, which are not identical but the epb_packetid can be used to correlate them.
        /// </summary>
        public ulong? PacketID { get; }

        /// <summary>
        /// The epb_queue option is a 32-bit unsigned integer that identifies on which queue of the interface the specific packet was received.
        /// </summary>
        public uint? Queue { get; }

        /// <summary>
        /// The epb_verdict option stores a verdict of the packet. The verdict indicates what would be done with the packet after processing it. For example, a firewall could drop the packet. This verdict can be set by various components, i.e. Hardware, Linux's eBPF TC or XDP framework, etc. etc. The first octet specifies the verdict type, while the following octets contain the actual verdict data, whose size depends on the verdict type, and hence from the value in the first octet. The verdict type can be: Hardware (type octet = 0, size = variable), Linux_eBPF_TC (type octet = 1, size = 8 (64-bit unsigned integer), value = TC_ACT_* as defined in the Linux pck_cls.h include), Linux_eBPF_XDP (type octet = 2, size = 8 (64-bit unsigned integer), value = xdp_action as defined in the Linux pbf.h include).
        /// </summary>
        public byte[] Verdict { get; }

        public EnhancedPacketOptions(in List<string> comments = null, in PacketFlags? flags = null, in ulong? dropCount = null, in List<Hash> hashes = null,
            in uint? packetID = null, in uint? queue = null, in byte[] verdict = null)
        {
            Comments = comments;
            Flags = flags;
            Hashes = hashes;
            DropCount = dropCount;
            PacketID = packetID;
            Queue = queue;
            Verdict = verdict;
        }

        public EnhancedPacketOptions(in ReadOnlySpan<byte> bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");

            Comments = null;
            Flags = null;
            Hashes = null;
            DropCount = null;
            PacketID = null;
            Queue = null;
            Verdict = null;

            foreach ((ushort key, byte[] value) in ReadOptions(bytes, out _))
            {
                switch (key)
                {
                    case (ushort)EnhancedPacketOptionCodes.Comment:
                        Comments ??= new List<string>();
                        Comments.Add(Encoding.UTF8.GetString(value));
                        break;

                    case (ushort)EnhancedPacketOptionCodes.Flags:
                        if (value.Length == 4)
                        {
                            uint packetBlockFlags = (BitConverter.ToUInt32(value, 0));
                            Flags = new PacketFlags(packetBlockFlags);
                        }
                        else
                            throw new ArgumentException($"Enhanced Packet Flags is {value.Length} bytes instead of the expected 4 bytes.");
                        break;

                    case (ushort)EnhancedPacketOptionCodes.Hash:
                        Hashes ??= new List<Hash>();
                        Hashes.Add(new Hash(value));
                        break;

                    case (ushort)EnhancedPacketOptionCodes.DropCount:
                        if (value.Length == 8)
                            DropCount = (BitConverter.ToUInt64(value, 0));
                        else
                            throw new ArgumentException($"Enhanced Packet Drop Count is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)EnhancedPacketOptionCodes.PacketID:
                        if (value.Length == 8)
                            PacketID = (BitConverter.ToUInt64(value, 0));
                        else
                            throw new ArgumentException($"Enhanced Packet Packet ID is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)EnhancedPacketOptionCodes.Queue:
                        if (value.Length == 4)
                            Queue = (BitConverter.ToUInt32(value, 0));
                        else
                            throw new ArgumentException($"Enhanced Packet Queue is {value.Length} bytes instead of the expected 4 bytes.");
                        break;

                    case (ushort)EnhancedPacketOptionCodes.Verdict:
                        Verdict = value;
                        break;

                    case (ushort)EnhancedPacketOptionCodes.EndOfOptions:
                        break;

                    default:
                        Debug.WriteLine($"Unknown Enhanced Packet Options Code of {key}.");
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
                        bytes.AddRange(ConvertOptionFieldToBytes((ushort)EnhancedPacketOptionCodes.Comment, commentValueBytes)); 
                }
            }

            if (Flags.HasValue)
            {
                byte[] flagsValueBytes = BitConverter.GetBytes(Flags.Value.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)EnhancedPacketOptionCodes.Flags, flagsValueBytes));
            }

            if (Hashes != null)
            {
                foreach (Hash hash in Hashes)
                {
                    byte[] hashValueBytes = hash.ToBytes();
                    if (hashValueBytes.Length <= UInt16.MaxValue)
                        bytes.AddRange(ConvertOptionFieldToBytes((ushort)EnhancedPacketOptionCodes.Hash, hashValueBytes));
                }
            }

            if (DropCount.HasValue)
            {
                byte[] dropCountValueBytes = BitConverter.GetBytes(DropCount.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)EnhancedPacketOptionCodes.DropCount, dropCountValueBytes));
            }

            if (PacketID.HasValue)
            {
                byte[] packetIDValueBytes = BitConverter.GetBytes(PacketID.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)EnhancedPacketOptionCodes.PacketID, packetIDValueBytes));
            }

            if (Queue.HasValue)
            {
                byte[] queueValueBytes = BitConverter.GetBytes(Queue.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)EnhancedPacketOptionCodes.Queue, queueValueBytes));
            }

            if (Verdict != null)
            {
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)EnhancedPacketOptionCodes.Verdict, Verdict));
            }

            if (bytes.Count > 0)
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)EnhancedPacketOptionCodes.EndOfOptions, Array.Empty<byte>()));

            return bytes.ToArray();
        }
    }
}