namespace CaptureNET.PcapNG.Options.Helpers
{
    internal enum EnhancedPacketOptionCodes : ushort
    {
        EndOfOptions = 0,
        Comment = 1,
        Flags = 2,
        Hash = 3,
        DropCount = 4,
        PacketID = 5,
        Queue = 6,
        Verdict = 7
    }
}
