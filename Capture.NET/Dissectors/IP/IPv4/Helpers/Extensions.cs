namespace CaptureNET.Dissectors.IP.IPv4.Helpers
{
    public static class Extensions
    {
        public static bool IsFragment(this IPv4Packet ipv4Packet)
        {
            if (ipv4Packet.FragmentOffset == 0 && !ipv4Packet.Flags.MoreFragments)
                return false;

            return true;
        }
    }
}