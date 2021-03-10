namespace CaptureNET.Dissectors.IP.IPv4
{
    public struct IPv4Flags
    {
        public bool Reserved;
        public bool DontFragment;
        public bool MoreFragments;

        public IPv4Flags(byte flags)
        {
            Reserved = ((flags & 4) == 4);
            DontFragment = ((flags & 2) == 2);
            MoreFragments = ((flags & 1) == 1);
        }
    }
}
