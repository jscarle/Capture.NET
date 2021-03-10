using System;
using System.Collections.Generic;
using System.Net;

namespace CaptureNET.PcapNG.Common
{
    public readonly struct IPv6Address
    {
        private readonly IPAddress _address;
        public IPAddress Address => _address;

        private readonly byte _prefix;
        public byte Prefix => _prefix;

        public IPv6Address(in byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");
            if (bytes.Length != 17)
                throw new ArgumentException($"Length of {nameof(bytes)} must be 17.");

            _address = new IPAddress(bytes[..16]);
            _prefix = bytes[16];
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(_address.GetAddressBytes());
            bytes.Add(_prefix);
            return bytes.ToArray();
        }
    }
}