using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace CaptureNET.Dissectors.SDP.Helpers
{
    public static class Extensions
    {
        public static IPAddress Plus(this IPAddress ipAddress, uint count)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                byte[] ipAddressBytes = ipAddress.GetAddressBytes();
                uint nextIPAddress = BinaryPrimitives.ReadUInt32BigEndian(ipAddressBytes.AsSpan()[12..]) + count;
                byte[] changedBytes = new byte[4];
                BinaryPrimitives.WriteUInt32BigEndian(changedBytes, nextIPAddress);
                Buffer.BlockCopy(changedBytes, 0, ipAddressBytes, ipAddressBytes.Length - 4, 4);
                return new IPAddress(ipAddressBytes);
            }
            else
            {
                byte[] ipAddressBytes = ipAddress.GetAddressBytes();
                uint nextIPAddress = BinaryPrimitives.ReadUInt32BigEndian(ipAddressBytes) + count;
                BinaryPrimitives.WriteUInt32BigEndian(ipAddressBytes, nextIPAddress);
                return new IPAddress(ipAddressBytes);
            }
        }
    }
}