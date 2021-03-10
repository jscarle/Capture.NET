using System;

namespace CaptureNET.Common
{
    public interface IGenericPacket
    {
        ReadOnlySpan<byte> PacketData { get; }

        byte[] ToBytes();
    }
}
