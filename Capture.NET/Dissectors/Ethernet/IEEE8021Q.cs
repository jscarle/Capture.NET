using System;
using System.Buffers.Binary;

namespace CaptureNET.Dissectors.Ethernet
{
    public readonly struct IEEE8021Q
    {
        public readonly byte CoS;
        public readonly bool DropEligible;
        public readonly ushort VLANID;

        public IEEE8021Q(in ReadOnlySpan<byte> bytes)
        {
            ushort tag = BinaryPrimitives.ReadUInt16BigEndian(bytes);
            CoS = (byte)((tag & 0xE000) >> 13);
            DropEligible = (tag & (1 << 13)) != 0;
            VLANID = (ushort)(tag & 0xFFF);
        }
    }
}