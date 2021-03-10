using System;
using System.Buffers.Binary;

namespace CaptureNET.Dissectors.Ethernet
{
    public readonly struct IEEE8022SNAP
    {
        public readonly byte[] OUI;
        public readonly ushort ProtocolID;

        public IEEE8022SNAP(ReadOnlySpan<byte> bytes)
        {
            OUI = bytes.Slice(0, 3).ToArray();
            ProtocolID = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(3, 2));
        }
    }
}