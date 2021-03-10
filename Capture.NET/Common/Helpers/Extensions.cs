using System;

namespace CaptureNET.Common.Helpers
{
    public static class Extensions
    {
        public static ReadOnlySpan<byte> Forward(this ReadOnlySpan<byte> bytes, ref int offset, in int count)
        {
            ReadOnlySpan<byte> span = bytes.Slice(offset, count);
            offset += count;
            return span;
        }
    }
}
