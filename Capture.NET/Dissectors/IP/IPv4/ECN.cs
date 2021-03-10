namespace CaptureNET.Dissectors.IP.IPv4
{
    public enum ECN : byte
    {
        NotCapable = 0,
        Capable1 = 1,
        Capable0 = 2,
        CongestionExperienced = 3
    }
}
