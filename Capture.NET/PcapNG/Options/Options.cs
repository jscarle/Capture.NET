using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using CaptureNET.Common.Helpers;

namespace CaptureNET.PcapNG.Options
{
    public abstract class Options
    {
        private const int AlignmentBoundary = 4;
        private const ushort EndOfOption = 0;

        private protected static List<KeyValuePair<ushort, byte[]>> ReadOptions(in ReadOnlySpan<byte> bytes, out int finalOffset)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");

            List<KeyValuePair<ushort, byte[]>> options = new List<KeyValuePair<ushort, byte[]>>();

            int offset = 0;
            while (true)
            {
                ushort optionCode = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Forward(ref offset, 2));
                ushort valueLength = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Forward(ref offset, 2));

                if (optionCode == EndOfOption)
                    break;

                if (valueLength > 0)
                {
                    byte[] valueBytes = bytes.Forward(ref offset, valueLength).ToArray();
                    ForwardToBoundary(valueLength, ref offset);
                    options.Add(new KeyValuePair<ushort, byte[]>(optionCode, valueBytes));
                }
            }

            finalOffset = offset;

            return options;
        }

        private static byte[] AlignmentBytes(in int unalignedLength)
        {
            return new byte[(AlignmentBoundary - unalignedLength % AlignmentBoundary) % AlignmentBoundary];
        }

        private static void ForwardToBoundary(in int unalignedLength, ref int offset)
        {
            offset += ((AlignmentBoundary - unalignedLength % AlignmentBoundary) % AlignmentBoundary);
        }

        private protected static byte[] ConvertOptionFieldToBytes(in ushort optionCode, in byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException($"{nameof(value)} cannot be null.");
            if (value.Length == 0 && optionCode != 0)
                throw new ArgumentException($"{nameof(value.Length)} can only be 0 for {nameof(optionCode)} 0.");
            if (value.Length > UInt16.MaxValue)
                throw new IndexOutOfRangeException($"{nameof(value.Length)} is larger than {UInt16.MaxValue}.");

            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(optionCode));
            bytes.AddRange(BitConverter.GetBytes(((ushort)value.Length)));
            if (value.Length > 0)
            {
                bytes.AddRange(value);
                bytes.AddRange(AlignmentBytes(value.Length));
            }

            return bytes.ToArray();
        }
    }
}