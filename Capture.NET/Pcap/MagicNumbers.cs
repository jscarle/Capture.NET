namespace CaptureNET.Pcap
{
    public enum MagicNumbers : uint
    {
        NanosecondIdentical = 0xa1b23c4d,
        NanosecondSwapped = 0x4d3cb2a1,
        MicrosecondIdentical = 0xa1b2c3d4,
        MicrosecondSwapped = 0xd4c3b2a1
    }
}
