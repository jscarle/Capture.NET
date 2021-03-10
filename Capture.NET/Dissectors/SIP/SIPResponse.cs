using System;
using CaptureNET.Dissectors.UDP;

namespace CaptureNET.Dissectors.SIP
{
    public sealed class SIPResponse : SIPMessage
    {
        private readonly ushort _statusCode;
        public ushort StatusCode => _statusCode;

        private readonly string _reason;
        public string ReasonPhrase => _reason;

        public SIPResponse(in UDPDatagram udpDatagram)
            : base(udpDatagram)
        {
            _statusCode = UInt16.Parse(_startLine[1]);
            _reason = _startLine[2];
        }
    }
}
