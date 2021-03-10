using System.Collections.Generic;

namespace CaptureNET.Dissectors.RTP
{
    public static class RTPFilters
    {
        public static bool? Padding { get; set; }
        public static bool? Extension { get; set; }
        public static byte? CSRCCount { get; set; }
        public static bool? Marker { get; set; }
        public static List<byte> PayloadTypes { get; set; }
    }
}
