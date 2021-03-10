using System;
using System.Collections.Generic;

namespace CaptureNET.PcapNG.Common
{
    public readonly struct Hash
    {
        public HashAlgorithm Algorithm { get; }
        public byte[] Value { get; }

        public Hash(in byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");
            if (bytes.Length < 2)
                throw new ArgumentException($"Length {nameof(bytes)} is less than 2.");

            uint algorithm = bytes[0];
#if DEBUG
            if (!Enum.IsDefined(typeof(HashAlgorithm), algorithm))
                throw new NotImplementedException($"Unknown Hash Algorithm of 0x{algorithm:x2}.");
#endif
            Algorithm = (HashAlgorithm)algorithm;

            Value = bytes[1..];
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Algorithm);
            bytes.AddRange(Value);
            return bytes.ToArray();
        }
    }
}