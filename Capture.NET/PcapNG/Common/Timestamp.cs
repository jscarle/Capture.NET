using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace CaptureNET.PcapNG.Common
{
    public readonly struct Timestamp
    {
        private readonly uint _timestampHigh;
        public uint TimestampHigh => _timestampHigh;

        private readonly uint _timestampLow;
        public uint TimestampLow => _timestampLow;

        private readonly ulong _timestamp;
        public ulong Value => _timestamp;

        private readonly uint _seconds;
        public uint Seconds => _seconds;

        private readonly uint _microseconds;
        public uint Microseconds => _microseconds;

        private readonly long _ticks;
        public DateTime LocalTime => new DateTime(EpochTicks + _ticks, DateTimeKind.Utc).ToLocalTime();

        private const ulong MicrosecondsPerSecond = 1000000UL;
        private const ulong HighFactor = 4294967296UL;
        private const long EpochTicks = 621355968000000000L;
        private const long TicksPerMicrosecond = 10L;

        public Timestamp(in ReadOnlySpan<byte> bytes)
        {
            _timestampHigh = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(0, 4));
            _timestampLow = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(4, 4));
            _timestamp = (_timestampHigh * HighFactor) + _timestampLow;

            _seconds = (uint)(_timestamp / MicrosecondsPerSecond);
            _microseconds = (uint)(_timestamp % MicrosecondsPerSecond);

            _ticks = (long)_timestamp * TicksPerMicrosecond;
        }

        public Timestamp(in uint seconds, in uint microseconds)
        {
            _seconds = seconds;
            _microseconds = microseconds;

            _timestamp = seconds * MicrosecondsPerSecond + microseconds;
            _timestampHigh = (uint)(_timestamp / HighFactor);
            _timestampLow = (uint)(_timestamp % HighFactor);

            _ticks = (long)_timestamp * TicksPerMicrosecond;
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(_timestampHigh));
            bytes.AddRange(BitConverter.GetBytes(_timestampLow));
            return bytes.ToArray();
        }
    }
}