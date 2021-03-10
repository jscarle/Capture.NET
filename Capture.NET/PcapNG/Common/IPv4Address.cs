using System;
using System.Collections.Generic;
using System.Net;

namespace CaptureNET.PcapNG.Common
{
    public readonly struct IPv4Address
    {
        private readonly IPAddress _address;
        public IPAddress Address => _address;

        private readonly IPAddress _mask;
        public IPAddress Mask => _mask;

        public IPv4Address(in byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");
            if (bytes.Length != 8)
                throw new ArgumentException($"Length of {nameof(bytes)} must be 8.");

            _address = new IPAddress(bytes[..4]);
            _mask = new IPAddress(bytes[4..]);
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(_address.GetAddressBytes());
            bytes.AddRange(_mask.GetAddressBytes());
            return bytes.ToArray();
        }
    }
}