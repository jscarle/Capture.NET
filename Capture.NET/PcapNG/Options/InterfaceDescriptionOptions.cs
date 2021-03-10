using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using CaptureNET.PcapNG.Common;
using CaptureNET.PcapNG.Options.Helpers;

namespace CaptureNET.PcapNG.Options
{
    public sealed class InterfaceDescriptionOptions : Options
    {
        /// <summary>
        /// The opt_comment option is a UTF-8 string containing human-readable comment text that is associated to the current block. Line separators SHOULD be a carriage-return + linefeed ('\r\n') or just linefeed ('\n'); either form may appear and be considered a line separator. The string is not zero-terminated.
        /// </summary>
        public List<string> Comments { get; }

        /// <summary>
        /// The if_name option is a UTF-8 string containing the name of the device used to capture data. The string is not zero-terminated.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The if_description option is a UTF-8 string containing the description of the device used to capture data. The string is not zero-terminated.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The if_IPv4addr option is an IPv4 network address and corresponding netmask for the interface. The first four octets are the IP address, and the next four octets are the netmask. This option can be repeated multiple times within the same Interface Description Block when multiple IPv4 addresses are assigned to the interface. Note that the IP address and netmask are both treated as four octets, one for each octet of the address or mask; they are not 32-bit numbers, and thus the endianness of the SHB does not affect this field's value.
        /// </summary>
        public List<IPv4Address> IPv4Address { get; }

        /// <summary>
        /// The if_IPv6addr option is an IPv6 network address and corresponding prefix length for the interface. The first 16 octets are the IP address and the next octet is the prefix length. This option can be repeated multiple times within the same Interface Description Block when multiple IPv6 addresses are assigned to the interface.
        /// </summary>
        public List<IPv6Address> IPv6Address { get; }

        /// <summary>
        /// The if_MACaddr option is the Interface Hardware MAC address (48 bits), if available.
        /// </summary>
        public PhysicalAddress MACAddress { get; }

        /// <summary>
        /// The if_EUIaddr option is the Interface Hardware EUI address (64 bits), if available.
        /// </summary>
        public byte[] EUIAddress { get; }

        /// <summary>
        /// The if_speed option is a 64-bit unsigned value indicating the interface speed, in bits per second.
        /// </summary>
        public long? Speed { get; }

        /// <summary>
        /// The if_tsresol option identifies the resolution of timestamps. If the Most Significant Bit is equal to zero, the remaining bits indicates the resolution of the timestamp as a negative power of 10 (e.g. 6 means microsecond resolution, timestamps are the number of microseconds since 1970-01-01 00:00:00 UTC). If the Most Significant Bit is equal to one, the remaining bits indicates the resolution as negative power of 2 (e.g. 10 means 1/1024 of second). If this option is not present, a resolution of 10^-6 is assumed (i.e. timestamps have the same resolution of the standard 'libpcap' timestamps).
        /// </summary>
        public byte? TimestampResolution { get; }

        /// <summary>
        /// The if_tzone option identifies the time zone for GMT support (TODO: specify better).
        /// </summary>
        public int? Timezone { get; }

        /// <summary>
        /// The if_filter option identifies the filter (e.g. "capture only TCP traffic") used to capture traffic. The first octet of the Option Data keeps a code of the filter used (e.g. if this is a libpcap string, or BPF bytecode, and more). More details about this format will be presented in Appendix XXX (TODO). (TODO: better use different options for different fields? e.g. if_filter_pcap, if_filter_bpf)
        /// </summary>
        public byte[] Filter { get; }

        /// <summary>
        /// The if_os option is a UTF-8 string containing the name of the operating system of the machine in which this interface is installed. This can be different from the same information that can be contained by the Section Header Block (Section 4.1) because the capture can have been done on a remote machine. The string is not zero-terminated.
        /// </summary>
        public string OperatingSystem { get; }

        /// <summary>
        /// The if_fcslen option is an 8-bit unsigned integer value that specifies the length of the Frame Check Sequence (in bits) for this interface. For link layers whose FCS length can change during time, the Packet Block epb_flags Option can be used in each Packet Block (see Section 4.3.1).
        /// </summary>
        public byte? FCSLength { get; }

        /// <summary>
        /// The if_tsoffset option is a 64-bit signed integer value that specifies an offset (in seconds) that must be added to the timestamp of each packet to obtain the absolute timestamp of a packet. If the option is missing, the timestamps stored in the packet MUST be considered absolute timestamps. The time zone of the offset can be specified with the option if_tzone. TODO: won't a if_tsoffset_low for fractional second offsets be useful for highly synchronized capture systems?
        /// </summary>
        public long? TimestampOffset { get; }

        /// <summary>
        /// The if_hardware option is a UTF-8 string containing the description of the interface hardware. The string is not zero-terminated.
        /// </summary>
        public string Hardware { get; }

        /// <summary>
        /// The if_txrxspeeds option is a 64-bit unsigned value indicating the interface transmit speed in bits per second.
        /// </summary>
        public long? TransmitSpeed { get; }

        /// <summary>
        /// The if_rxspeed option is a 64-bit unsigned value indicating the interface receive speed, in bits per second.
        /// </summary>
        public long? ReceiveSpeed { get; }

        public InterfaceDescriptionOptions(in List<string> comments = null, in string name = null, in string description = null, in List<IPv4Address> ipv4Address = null,
            List<IPv6Address> ipv6Address = null, in PhysicalAddress macAddress = null, in byte[] euiAddress = null, in long? speed = null, in byte? timestampResolution = 6,
            int? timezone = null, in byte[] filter = null, in string operatingSystem = null, in byte? frameCheckSequence = null, in long? timestampOffset = null,
            in string hardware = null, in long? transmitSpeed = null, in long? receiveSpeed = null)
        {
            if (euiAddress != null && euiAddress.Length != 8)
                throw new ArgumentException($"Length of {nameof(euiAddress)} must be 8.");

            Comments = comments;
            Name = name;
            Description = description;
            IPv4Address = ipv4Address;
            IPv6Address = ipv6Address;
            MACAddress = macAddress;
            EUIAddress = euiAddress;
            Speed = speed;
            TimestampResolution = timestampResolution;
            Timezone = timezone;
            Filter = filter;
            OperatingSystem = operatingSystem;
            FCSLength = frameCheckSequence;
            TimestampOffset = timestampOffset;
            Hardware = hardware;
            TransmitSpeed = transmitSpeed;
            ReceiveSpeed = receiveSpeed;
        }

        public InterfaceDescriptionOptions(in ReadOnlySpan<byte> bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");

            Comments = null;
            Name = null;
            Description = null;
            IPv4Address = null;
            IPv6Address = null;
            MACAddress = null;
            EUIAddress = null;
            Speed = null;
            TimestampResolution = null;
            Timezone = null;
            Filter = null;
            OperatingSystem = null;
            FCSLength = null;
            TimestampOffset = null;
            Hardware = null;
            TransmitSpeed = null;
            ReceiveSpeed = null;

            foreach ((ushort key, byte[] value) in ReadOptions(bytes, out _))
            {
                switch (key)
                {
                    case (ushort)InterfaceDescriptionOptionCodes.Comment:
                        Comments ??= new List<string>();
                        Comments.Add(Encoding.UTF8.GetString(value));
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.Name:
                        Name = Encoding.UTF8.GetString(value);
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.Description:
                        Description = Encoding.UTF8.GetString(value);
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.IPv4Address:
                        if (value.Length == 8)
                        {
                            IPv4Address ??= new List<IPv4Address>();
                            IPv4Address.Add(new IPv4Address(value));
                        }
                        else
                            throw new ArgumentException($"Interface Description IPv4 Address is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.IPv6Address:
                        if (value.Length == 16)
                        {
                            IPv6Address ??= new List<IPv6Address>();
                            IPv6Address.Add(new IPv6Address(value));
                        }
                        else
                            throw new ArgumentException($"Interface Description IPv6 Address is {value.Length} bytes instead of the expected 16 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.MACAddress:
                        if (value.Length == 6)
                            MACAddress = new PhysicalAddress(value);
                        else
                            throw new ArgumentException($"Interface Description MAC Address is {value.Length} bytes instead of the expected 6 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.EUIAddress:
                        if (value.Length == 8)
                            EUIAddress = value;
                        else
                            throw new ArgumentException($"Interface Description EUI Address is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.Speed:
                        if (value.Length == 8)
                            Speed = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Description Speed is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.TimestampResolution:
                        if (value.Length == 1)
                            TimestampResolution = value[0];
                        else
                            throw new ArgumentException($"Interface Description Timestamp Resolution is {value.Length} bytes instead of the expected 1 byte.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.Timezone:
                        if (value.Length == 4)
                            Timezone = (BitConverter.ToInt32(value, 0)); // GMT offset
                        else
                            throw new ArgumentException($"Interface Description Timezone is {value.Length} bytes instead of the expected 4 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.Filter:
                        Filter = value;
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.OperatingSystem:
                        OperatingSystem = Encoding.UTF8.GetString(value);
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.FrameCheckSequence:
                        if (value.Length == 1)
                            FCSLength = value[0];
                        else
                            throw new ArgumentException($"Interface Description Frame Check Sequence is {value.Length} bytes instead of the expected 1 byte.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.TimestampOffset:
                        if (value.Length == 8)
                            TimestampOffset = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Description Timestamp Offset is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.Hardware:
                        Hardware = Encoding.UTF8.GetString(value);
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.TransmitSpeed:
                        if (value.Length == 8)
                            TransmitSpeed = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Description Transmit Speed is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.ReceiveSpeed:
                        if (value.Length == 8)
                            ReceiveSpeed = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Description Receive Speed is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceDescriptionOptionCodes.EndOfOptions:
                        break;

                    default:
                        Debug.WriteLine($"Unknown Interface Description Options Code of {key}.");
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
                        bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.Comment, commentValueBytes));
                }
            }

            if (Name != null)
            {
                byte[] nameValueBytes = Encoding.UTF8.GetBytes(Name);
                if (nameValueBytes.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.Name, nameValueBytes));
            }

            if (Description != null)
            {
                byte[] descriptionValueBytes = Encoding.UTF8.GetBytes(Description);
                if (descriptionValueBytes.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.Description, descriptionValueBytes));
            }

            if (IPv4Address != null)
            {
                foreach (IPv4Address ipv4Address in IPv4Address)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.IPv4Address, ipv4Address.ToBytes()));
            }

            if (IPv6Address != null)
            {
                foreach (IPv6Address ipv6Address in IPv6Address)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.IPv6Address, ipv6Address.ToBytes()));
            }

            if (MACAddress != null)
            {
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.MACAddress, MACAddress.GetAddressBytes()));
            }

            if (EUIAddress != null)
            {
                if (EUIAddress.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.EUIAddress, EUIAddress));
            }

            if (Speed.HasValue)
            {
                byte[] speedValueBytes = BitConverter.GetBytes(Speed.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.Speed, speedValueBytes));
            }

            if (TimestampResolution.HasValue)
            {
                byte[] timestampValueBytes = { TimestampResolution.Value };
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.TimestampResolution, timestampValueBytes));
            }

            if (Timezone.HasValue)
            {
                byte[] timezoneValueBytes = BitConverter.GetBytes(Timezone.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.Timezone, timezoneValueBytes));
            }

            if (Filter != null)
            {
                if (Filter.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.Filter, Filter));
            }

            if (OperatingSystem != null)
            {
                byte[] operatingSystemValueBytes = Encoding.UTF8.GetBytes(OperatingSystem);
                if (operatingSystemValueBytes.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.OperatingSystem, operatingSystemValueBytes));
            }

            if (FCSLength.HasValue)
            {
                byte[] frameCheckSequenceValueBytes = { FCSLength.Value };
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.FrameCheckSequence, frameCheckSequenceValueBytes));
            }

            if (TimestampOffset.HasValue)
            {
                byte[] timestampOffsetValueBytes = BitConverter.GetBytes(TimestampOffset.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.TimestampOffset, timestampOffsetValueBytes));
            }

            if (Hardware != null)
            {
                byte[] hardwareValueBytes = Encoding.UTF8.GetBytes(Hardware);
                if (hardwareValueBytes.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.Hardware, hardwareValueBytes));
            }

            if (TransmitSpeed.HasValue)
            {
                byte[] transmitSpeedValueBytes = BitConverter.GetBytes(TransmitSpeed.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.TransmitSpeed, transmitSpeedValueBytes));
            }

            if (ReceiveSpeed.HasValue)
            {
                byte[] receiveSpeedValueBytes = BitConverter.GetBytes(ReceiveSpeed.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.ReceiveSpeed, receiveSpeedValueBytes));
            }

            if (bytes.Count > 0)
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceDescriptionOptionCodes.EndOfOptions, Array.Empty<byte>()));

            return bytes.ToArray();
        }
    }
}