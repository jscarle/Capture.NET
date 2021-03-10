using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CaptureNET.PcapNG.Options.Helpers;

namespace CaptureNET.PcapNG.Options
{
    public class SectionHeaderOptions : Options
    {
        /// <summary>
        /// The opt_comment option is a UTF-8 string containing human-readable comment text that is associated to the current block. Line separators SHOULD be a carriage-return + linefeed ('\r\n') or just linefeed ('\n'); either form may appear and be considered a line separator. The string is not zero-terminated.
        /// </summary>
        public List<string> Comments { get; }

        /// <summary>
        /// The shb_hardware option is a UTF-8 string containing the description of the hardware used to create this section. The string is not zero-terminated.
        /// </summary>
        public string Hardware { get; }

        /// <summary>
        /// The shb_os option is a UTF-8 string containing the name of the operating system used to create this section. The string is not zero-terminated.
        /// </summary>
        public string OperatingSystem { get; }

        /// <summary>
        /// The shb_userappl option is a UTF-8 string containing the name of the application used to create this section. The string is not zero-terminated.
        /// </summary>
        public string UserApplication { get; }

        public SectionHeaderOptions(in List<string> comments = null, in string hardware = null, in string operatingSystem = null, in string userApplication = null)
        {
            Comments = comments;
            Hardware = hardware;
            OperatingSystem = operatingSystem;
            UserApplication = userApplication;
        }

        public SectionHeaderOptions(in ReadOnlySpan<byte> bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");

            Comments = null;
            Hardware = null;
            OperatingSystem = null;
            UserApplication = null;

            foreach ((ushort key, byte[] value) in ReadOptions(bytes, out _))
            {
                switch (key)
                {
                    case (ushort)SectionHeaderOptionCodes.Comment:
                        Comments ??= new List<string>();
                        Comments.Add(Encoding.UTF8.GetString(value));
                        break;

                    case (ushort)SectionHeaderOptionCodes.Hardware:
                        Hardware = Encoding.UTF8.GetString(value);
                        break;

                    case (ushort)SectionHeaderOptionCodes.OperatingSystem:
                        OperatingSystem = Encoding.UTF8.GetString(value);
                        break;

                    case (ushort)SectionHeaderOptionCodes.UserApplication:
                        UserApplication = Encoding.UTF8.GetString(value);
                        break;

                    case (ushort)SectionHeaderOptionCodes.EndOfOptions:
                        break;

                    default:
                        Debug.WriteLine($"Unknown Section Header Options Code of {key}.");
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
                        bytes.AddRange(ConvertOptionFieldToBytes((ushort)SectionHeaderOptionCodes.Comment,
                            commentValueBytes));
                }
            }

            if (Hardware != null)
            {
                byte[] hardwareValue = Encoding.UTF8.GetBytes(Hardware);
                if (hardwareValue.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)SectionHeaderOptionCodes.Hardware, hardwareValue));
            }

            if (OperatingSystem != null)
            {
                byte[] systemValue = Encoding.UTF8.GetBytes(OperatingSystem);
                if (systemValue.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)SectionHeaderOptionCodes.OperatingSystem, systemValue));
            }

            if (UserApplication != null)
            {
                byte[] userAppValue = Encoding.UTF8.GetBytes(UserApplication);
                if (userAppValue.Length <= UInt16.MaxValue)
                    bytes.AddRange(ConvertOptionFieldToBytes((ushort)SectionHeaderOptionCodes.UserApplication, userAppValue));
            }

            if (bytes.Count > 0)
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)SectionHeaderOptionCodes.EndOfOptions, Array.Empty<byte>()));

            return bytes.ToArray();
        }
    }
}