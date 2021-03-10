namespace CaptureNET.PcapNG.Common
{
    public enum HashAlgorithm : byte
    {
        TwoSComplement = 0,
        XOR = 1,
        CRC32 = 2,
        MD5 = 3,
        SHA1 = 4,
        Toeplitz = 5
    }
}
