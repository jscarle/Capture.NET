using System;
using System.Net;
using System.Net.Sockets;

namespace CaptureNET.PcapNG.Options
{
    public readonly struct NameResolutionRecord
    {
        /// <summary>
        /// A UTF-8 string containing the name of the machine (DNS server) used to perform the name resolution.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The IPv4 or IPv6 address of the DNS server.
        /// </summary>
        public IPAddress IPAddress { get; }

        public NameResolutionRecord(in IPAddress ipAddress, in string description)
        {
            if (ipAddress == null)
                throw new ArgumentNullException($"{nameof(ipAddress)} cannot be null.");
            if (ipAddress.AddressFamily != AddressFamily.InterNetwork && ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException($"{nameof(ipAddress)} is not IP.");
            if (String.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException($"{nameof(description)} cannot be null or empty.");

            IPAddress = ipAddress;
            Description = description;
        }
    }
}