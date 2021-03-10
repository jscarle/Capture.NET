using System;

namespace CaptureNET.Dissectors.Ethernet
{
    public readonly struct IEEE8022LLC
    {
        public readonly byte DSAP;
        public readonly byte SSAP;
        public readonly byte Control;

        public IEEE8022LLC(in ReadOnlySpan<byte> bytes)
        {
            DSAP = bytes[0];
            SSAP = bytes[1];
            Control = bytes[2];
        }
    }
}