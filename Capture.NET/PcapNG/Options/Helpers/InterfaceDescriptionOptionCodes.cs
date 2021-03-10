namespace CaptureNET.PcapNG.Options.Helpers
{
    internal enum InterfaceDescriptionOptionCodes : ushort
    {
        EndOfOptions = 0,
        Comment = 1,
        Name = 2,
        Description = 3,
        IPv4Address = 4,
        IPv6Address = 5,
        MACAddress = 6,
        EUIAddress = 7,
        Speed = 8,
        TimestampResolution = 9,
        Timezone = 10,
        Filter = 11,
        OperatingSystem = 12,
        FrameCheckSequence = 13,
        TimestampOffset = 14,
        Hardware = 15,
        TransmitSpeed = 16,
        ReceiveSpeed = 17
    }
}
