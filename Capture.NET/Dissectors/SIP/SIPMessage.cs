using System;
using System.Collections.Immutable;
using System.Text;
using CaptureNET.Dissectors.SDP;
using CaptureNET.Dissectors.SIP.Helpers;
using CaptureNET.Dissectors.UDP;

namespace CaptureNET.Dissectors.SIP
{
    public abstract class SIPMessage
    {
        private readonly SIPUser _to;
        public SIPUser To => _to;

        private readonly SIPUser _from;
        public SIPUser From => _from;

        private readonly string _cseq;
        public string CSeq => _cseq;

        private readonly string _callID;
        public string CallID => _callID;

        private readonly int _maxForwards;
        public int MaxForwards => _maxForwards;

        private readonly ImmutableList<string> _via = ImmutableList<string>.Empty;
        public ImmutableList<string> Via => _via;
        
        private readonly ImmutableDictionary<string, ImmutableList<string>> _headers = ImmutableDictionary<string, ImmutableList<string>>.Empty;
        public ImmutableDictionary<string, ImmutableList<string>> Headers => _headers;

        private readonly string _body;
        public string Body => _body;

        private readonly SDPSessionDescription _sessionDescription;
        public SDPSessionDescription SessionDescription => _sessionDescription;

        private readonly SIPMessageRelated _related;
        public SIPMessageRelated Related => _related;

        private protected string[] _startLine;

        public SIPMessage(in UDPDatagram udpDatagram)
            : this(udpDatagram.Payload)
        {
            _related = new SIPMessageRelated(udpDatagram);
        }

        public SIPMessage(in ReadOnlySpan<byte> bytes)
        {
            string message = Encoding.UTF8.GetString(bytes);

            string[] messageParts = message.Split("\r\n\r\n");

            string[] lines = messageParts[0].Split("\r\n");

            _startLine = lines[0].Split(' ');

            for (int index = 1; index < lines.Length; index++)
            {
                int colonIndex = lines[index].IndexOf(':');
                if (colonIndex < 1)
                    continue;

                string headerName = lines[index].Substring(0, colonIndex).Trim();
                if (headerName.Length == 1)
                    headerName = headerName.ToLongFormHeader();
                string headerValue = lines[index][(colonIndex + 1)..].Trim();

                if (headerName.Equals("To", StringComparison.InvariantCultureIgnoreCase))
                    _to = new SIPUser(headerValue);
                else if (headerName.Equals("From", StringComparison.InvariantCultureIgnoreCase))
                    _from = new SIPUser(headerValue);
                else if (headerName.Equals("CSeq", StringComparison.InvariantCultureIgnoreCase))
                    _cseq = headerValue;
                else if (headerName.Equals("Call-ID", StringComparison.InvariantCultureIgnoreCase))
                    _callID = headerValue;
                else if (headerName.Equals("Max-Forwards", StringComparison.InvariantCultureIgnoreCase))
                    _maxForwards = Int32.Parse(headerValue);
                else if (headerName.Equals("Via", StringComparison.InvariantCultureIgnoreCase))
                    _via = _via.Add(headerValue);
                else
                {
                    string properCaseHeader = headerName.ToProperCaseHeader();
                    if (!_headers.ContainsKey(properCaseHeader))
                        _headers = _headers.Add(properCaseHeader, ImmutableList<string>.Empty);
                    _headers = _headers.SetItem(properCaseHeader, _headers[properCaseHeader].Add(headerValue));
                }
            }

            _body = messageParts.Length == 2 ? messageParts[1] : "";

            if (_body.Length > 3 && _headers.ContainsKey("Content-Type") && _headers["Content-Type"][0].Equals("application/sdp", StringComparison.InvariantCultureIgnoreCase))
                _sessionDescription = new SDPSessionDescription(_body);
            else
                _sessionDescription = null;
        }
    }
}
