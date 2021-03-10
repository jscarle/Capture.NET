using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CaptureNET.PcapNG.Common;
using CaptureNET.PcapNG.Options.Helpers;

namespace CaptureNET.PcapNG.Options
{
    public sealed class InterfaceStatisticsOptions : Options
    {
        /// <summary>
        /// The opt_comment option is a UTF-8 string containing human-readable comment text that is associated to the current block. Line separators SHOULD be a carriage-return + linefeed ('\r\n') or just linefeed ('\n'); either form may appear and be considered a line separator. The string is not zero-terminated.
        /// </summary>
        public List<string> Comments { get; }

        /// <summary>
        /// The isb_starttime option specifies the time the capture started; time will be stored in two blocks of four octets each. The format of the timestamp is the same as the one defined in the Packet Block (Section 4.3); the length of a unit of time is specified by the 'if_tsresol' option (see Figure 10) of the Interface Description Block referenced by this packet.
        /// </summary>
        public Timestamp? StartTime { get; }

        /// <summary>
        /// The isb_endtime option specifies the time the capture ended; time will be stored in two blocks of four octets each. The format of the timestamp is the same as the one defined in the Packet Block (Section 4.3); the length of a unit of time is specified by the 'if_tsresol' option (see Figure 10) of the Interface Description Block referenced by this packet.
        /// </summary>
        public Timestamp? EndTime { get; }

        /// <summary>
        /// The isb_ifrecv option specifies the 64-bit unsigned integer number of packets received from the physical interface starting from the beginning of the capture.
        /// </summary>
        public long? InterfaceReceived { get; }

        /// <summary>
        /// The isb_ifdrop option specifies the 64-bit unsigned integer number of packets dropped by the interface due to lack of resources starting from the beginning of the capture.
        /// </summary>
        public long? InterfaceDropped { get; }

        /// <summary>
        /// The isb_filteraccept option specifies the 64-bit unsigned integer number of packets accepted by filter starting from the beginning of the capture.
        /// </summary>
        public long? FilterAccepted { get; }

        /// <summary>
        /// The isb_osdrop option specifies the 64-bit unsigned integer number of packets dropped by the operating system starting from the beginning of the capture.
        /// </summary>
        public long? OperatingSystemDropped { get; }

        /// <summary>
        /// The isb_usrdeliv option specifies the 64-bit unsigned integer number of packets delivered to the user starting from the beginning of the capture. The value contained in this field can be different from the value 'isb_filteraccept - isb_osdrop' because some packets could still be in the OS buffers when the capture ended.
        /// </summary>
        public long? DeliveredToUser { get; }

        public InterfaceStatisticsOptions(in List<string> comments = null, in Timestamp? startTime = null, in Timestamp? endTime = null, in long? interfaceReceived = null,
            long? interfaceDrop = null, in long? filterAccept = null, in long? systemDrop = null, in long? deliveredToUser = null)
        {
            Comments = comments;
            StartTime = startTime;
            EndTime = endTime;
            InterfaceReceived = interfaceReceived;
            InterfaceDropped = interfaceDrop;
            FilterAccepted = filterAccept;
            OperatingSystemDropped = systemDrop;
            DeliveredToUser = deliveredToUser;
        }

        public InterfaceStatisticsOptions(in ReadOnlySpan<byte> bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");

            Comments = null;
            StartTime = null;
            EndTime = null;
            InterfaceReceived = null;
            InterfaceDropped = null;
            FilterAccepted = null;
            OperatingSystemDropped = null;
            DeliveredToUser = null;

            foreach ((ushort key, byte[] value) in ReadOptions(bytes, out _))
            {
                switch (key)
                {
                    case (ushort)InterfaceStatisticsOptionCodes.Comment:
                        Comments ??= new List<string>();
                        Comments.Add(Encoding.UTF8.GetString(value));
                        break;

                    case (ushort)InterfaceStatisticsOptionCodes.StartTime:
                        if (value.Length == 8)
                            StartTime = new Timestamp(value);
                        else
                            throw new ArgumentException($"Interface Statistics Start Time is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceStatisticsOptionCodes.EndTime:
                        if (value.Length == 8)
                            EndTime = new Timestamp(value);
                        else
                            throw new ArgumentException($"Interface Statistics End Time is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceStatisticsOptionCodes.InterfaceReceived:
                        if (value.Length == 8)
                            InterfaceReceived = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Statistics Interface Received is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceStatisticsOptionCodes.InterfaceDrop:
                        if (value.Length == 8)
                            InterfaceDropped = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Statistics Interface Dropped is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceStatisticsOptionCodes.FilterAccept:
                        if (value.Length == 8)
                            FilterAccepted = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Statistics Filter Accepted is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceStatisticsOptionCodes.SystemDrop:
                        if (value.Length == 8)
                            OperatingSystemDropped = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Statistics Operating System Dropped is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceStatisticsOptionCodes.DeliveredToUser:
                        if (value.Length == 8)
                            DeliveredToUser = (BitConverter.ToInt64(value, 0));
                        else
                            throw new ArgumentException($"Interface Statistics Delivered To User is {value.Length} bytes instead of the expected 8 bytes.");
                        break;

                    case (ushort)InterfaceStatisticsOptionCodes.EndOfOptions:
                        break;

                    default:
                        Debug.WriteLine($"Unknown Interface Statistics Options Code of {key}.");
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
                        bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.Comment,
                            commentValueBytes));
                }
            }

            if (StartTime != null)
            {
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.StartTime, StartTime?.ToBytes()));
            }

            if (EndTime != null)
            {
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.EndTime, EndTime?.ToBytes()));
            }

            if (InterfaceReceived.HasValue)
            {
                byte[] interfaceReceivedValueBytes = BitConverter.GetBytes(InterfaceReceived.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.InterfaceReceived, interfaceReceivedValueBytes));
            }

            if (InterfaceDropped.HasValue)
            {
                byte[] interfaceDropValueBytes = BitConverter.GetBytes(InterfaceDropped.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.InterfaceDrop, interfaceDropValueBytes));
            }

            if (FilterAccepted.HasValue)
            {
                byte[] filterAcceptValueBytes = BitConverter.GetBytes(FilterAccepted.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.FilterAccept, filterAcceptValueBytes));
            }

            if (OperatingSystemDropped.HasValue)
            {
                byte[] systemDropValueBytes = BitConverter.GetBytes(OperatingSystemDropped.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.SystemDrop, systemDropValueBytes));
            }

            if (DeliveredToUser.HasValue)
            {
                byte[] deliveredToUserValueBytes = BitConverter.GetBytes(DeliveredToUser.Value);
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.DeliveredToUser, deliveredToUserValueBytes));
            }

            if (bytes.Count > 0)
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)InterfaceStatisticsOptionCodes.EndOfOptions, Array.Empty<byte>()));

            return bytes.ToArray();
        }
    }
}