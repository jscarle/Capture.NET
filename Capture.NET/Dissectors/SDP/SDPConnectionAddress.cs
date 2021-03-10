using System.Net;

namespace CaptureNET.Dissectors.SDP
{
    public readonly struct SDPConnectionAddress
    {
        private readonly IPAddress _ipAddress;
        public IPAddress IPAddress => _ipAddress;

        private readonly ushort _ttl;
        public ushort TTL => _ttl;

        public SDPConnectionAddress(in IPAddress ipAddress, in ushort ttl)
        {
            _ipAddress = ipAddress;
            _ttl = ttl;
        }
    }
}