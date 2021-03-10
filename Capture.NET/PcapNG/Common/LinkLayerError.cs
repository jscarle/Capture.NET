using System;

namespace CaptureNET.PcapNG.Common
{
    [Flags]
    public enum LinkLayerError : uint
    {
        None = 0x00000000,
        CrcError = 0x01000000,
        PacketTooLongError = 0x02000000,
        PacketTooShortError = 0x04000000,
        WrongInterFrameGapError = 0x08000000,
        UnalignedFrameError = 0x10000000,
        StartFrameDelimiterError = 0x20000000,
        PreambleError = 0x40000000,
        SymbolError = 0x80000000
    }
}
