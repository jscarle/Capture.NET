using CaptureNET.Dissectors.UDP;

namespace CaptureNET.Dissectors.SIP
{
    public sealed class SIPRequest : SIPMessage
    {
        private readonly string _method;
        public string Method => _method;

        private readonly SIPURI _requestUri;
        public SIPURI RequestUri => _requestUri;

        public SIPRequest(in UDPDatagram udpDatagram)
            : base(udpDatagram)
        {
            _method = _startLine[0];
            _requestUri = new SIPURI(_startLine[1]);
        }
    }
}
