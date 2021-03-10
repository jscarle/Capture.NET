using System;
using System.Collections;

namespace CaptureNET.Dissectors.IP.IPv4
{
    public class IPv4FragmentHash : IEquatable<IPv4FragmentHash>
    {
        private readonly byte[] _sourceIPAddress;
        public byte[] SourceIPAddress => _sourceIPAddress;

        private readonly byte[] _destinationIPAddress;
        public byte[] DestinationIPAddress => _destinationIPAddress;

        private readonly ushort _id;
        public ushort ID => _id;

        public IPv4FragmentHash(byte[] sourceIPAddress, byte[] destinationIPAddress, ushort id)
        {
            _sourceIPAddress = sourceIPAddress;
            _destinationIPAddress = destinationIPAddress;
            _id = id;
        }

        public static bool operator ==(IPv4FragmentHash left, IPv4FragmentHash right)
        {
            return left is not null && left.Equals(right);
        }

        public static bool operator !=(IPv4FragmentHash left, IPv4FragmentHash right)
        {
            return left is not null && !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is IPv4FragmentHash other && Equals(other);
        }

        public bool Equals(IPv4FragmentHash other)
        {
            return other is not null && StructuralComparisons.StructuralEqualityComparer.Equals(_sourceIPAddress, other._sourceIPAddress) && StructuralComparisons.StructuralEqualityComparer.Equals(_destinationIPAddress, other._destinationIPAddress) && _id == other._id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = BitConverter.ToUInt32(_sourceIPAddress, 0).GetHashCode();
                hashCode = (hashCode * 397) ^ BitConverter.ToUInt32(_destinationIPAddress, 0).GetHashCode();
                hashCode = (hashCode * 397) ^ _id.GetHashCode();
                return hashCode;
            }
        }
    }
}
