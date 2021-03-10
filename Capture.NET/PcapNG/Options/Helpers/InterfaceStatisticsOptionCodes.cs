namespace CaptureNET.PcapNG.Options.Helpers
{
    internal enum InterfaceStatisticsOptionCodes : ushort
    {
        EndOfOptions = 0,
        Comment = 1,
        StartTime = 2,
        EndTime = 3,
        InterfaceReceived = 4,
        InterfaceDrop = 5,
        FilterAccept = 6,
        SystemDrop = 7,
        DeliveredToUser = 8
    }
}
