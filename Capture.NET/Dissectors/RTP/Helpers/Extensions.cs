using System;
using CaptureNET.Dissectors.UDP;

namespace CaptureNET.Dissectors.RTP.Helpers
{
    public static class Extensions
    {
        public static bool IsLikelyRTPPayload(this UDPDatagram udpDatagram)
        {
            if (udpDatagram.SourcePort % 2 != 0 || udpDatagram.DestinationPort % 2 != 0)
                return false;

            return IsLikelyRTPPayload(udpDatagram.Payload);
        }

        private static bool IsLikelyRTPPayload(in ReadOnlySpan<byte> payload)
        {
            byte version = (byte)((payload[0] & 0xC0) >> 6);
            bool padding = (payload[0] & 0x20) == 0x20;
            bool extension = (payload[0] & 0x10) == 0x10;
            byte csrcCount = (byte)(payload[0] & 0x0F);
            bool marker = (payload[1] & 0x80) == 0x80;
            byte payloadType = (byte)(payload[1] & 0x7F);

            // Version must be 2, STUN is often detected as RTP with a version value of 0
            if (version == 2 &&
                (!RTPFilters.Padding.HasValue || RTPFilters.Padding.Value == padding) &&
                (!RTPFilters.Extension.HasValue || RTPFilters.Extension.Value == extension) &&
                (!RTPFilters.CSRCCount.HasValue || RTPFilters.CSRCCount.Value == csrcCount) &&
                (!RTPFilters.Marker.HasValue || RTPFilters.Marker.Value == marker) &&
                (RTPFilters.PayloadTypes == null || RTPFilters.PayloadTypes.Contains(payloadType)))
                return true;

            return false;
        }
    }
}
