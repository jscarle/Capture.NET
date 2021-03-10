using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CaptureNET.PcapNG.Options.Helpers;

namespace CaptureNET.PcapNG.Options
{
    public sealed class NameResolutionOptions : Options
    {
        /// <summary>
        /// The opt_comment option is a UTF-8 string containing human-readable comment text that is associated to the current block. Line separators SHOULD be a carriage-return + linefeed ('\r\n') or just linefeed ('\n'); either form may appear and be considered a line separator. The string is not zero-terminated.
        /// </summary>
        public List<string> Comments { get; }

        /// <summary>
        /// The ns_dnsname option is a UTF-8 string containing the name of the machine (DNS server) used to perform the name resolution. The string is not zero-terminated.
        /// </summary>
        public string DNSName { get; }

        /// <summary>
        /// The ns_dnsIP4addr option specifies the IPv4 address of the DNS server. Note that the IP address is treated as four octets, one for each octet of the IP address; it is not a 32-bit word, and thus the endianness of the SHB does not affect this field's value.
        /// </summary>
        public IPAddress DNSIPv4Address { get; }

        /// <summary>
        /// The ns_dnsIP6addr option specifies the IPv6 address of the DNS server.
        /// </summary>
        public IPAddress DNSIPv6Address { get; }

        public NameResolutionOptions(in List<string> comments = null, in string dnsName = null, in IPAddress dnsIPv4Address = null, in IPAddress dnsIPv6Address = null)
        {
            if (dnsIPv4Address != null && dnsIPv4Address.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException($"{nameof(dnsIPv4Address)} is not IPv4.");
            if (dnsIPv6Address != null && dnsIPv6Address.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException($"{nameof(dnsIPv6Address)} is not IPv6.");

            Comments = comments;
            DNSName = dnsName;
            DNSIPv4Address = dnsIPv4Address;
            DNSIPv6Address = dnsIPv6Address;
        }

        public NameResolutionOptions(in ReadOnlySpan<byte> bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");

            Comments = null;
            DNSName = null;
            DNSIPv4Address = null;
            DNSIPv6Address = null;

            foreach ((ushort key, byte[] value) in ReadOptions(bytes, out _))
            {
                switch (key)
                {
                    case (ushort)NameResolutionOptionCodes.Comment:
                        Comments ??= new List<string>();
                        Comments.Add(Encoding.UTF8.GetString(value));
                        break;

                    case (ushort)NameResolutionOptionCodes.DNSName:
                        DNSName = Encoding.UTF8.GetString(value);
                        break;

                    case (ushort)NameResolutionOptionCodes.DNSIPv4Address:
                        if (value.Length == 4)
                            DNSIPv6Address = new IPAddress(value);
                        else
                            throw new ArgumentException($"Name Resolution DNS IPv4 Address is {value.Length} bytes instead of the expected 4 bytes.");
                        break;

                    case (ushort)NameResolutionOptionCodes.DNSIPv6Address:
                        if (value.Length == 16)
                            DNSIPv6Address = new IPAddress(value);
                        else
                            throw new ArgumentException($"Name Resolution DNS IPv6 Address is {value.Length} bytes instead of the expected 16 bytes.");
                        break;

                    case (ushort)NameResolutionOptionCodes.EndOfOptions:
                        break;

                    default:
                        Debug.WriteLine($"Unknown Name Resolution Options Code of {key}.");
                        break;
                }
            }
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();

            if (Comments != null)
            {
                foreach (string comment in Comments)
                {
                    byte[] commentValueBytes = Encoding.UTF8.GetBytes(comment);
                    if (commentValueBytes.Length <= UInt16.MaxValue)
                        bytes.AddRange(ConvertOptionFieldToBytes((ushort)NameResolutionOptionCodes.Comment,
                            commentValueBytes));
                }
            }

            if (DNSName != null)
            {
                byte[] dnsNameValueBytes = Encoding.UTF8.GetBytes(DNSName);
                if (dnsNameValueBytes.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)NameResolutionOptionCodes.DNSName, dnsNameValueBytes));
            }

            if (DNSIPv4Address != null)
            {
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)NameResolutionOptionCodes.DNSIPv4Address, DNSIPv4Address.GetAddressBytes()));
            }

            if (DNSIPv6Address != null)
            {
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)NameResolutionOptionCodes.DNSIPv6Address, DNSIPv6Address.GetAddressBytes()));
            }

            if (bytes.Count > 0)
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)NameResolutionOptionCodes.EndOfOptions, Array.Empty<byte>()));

            return bytes.ToArray();
        }
    }
}