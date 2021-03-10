using System.Net;

namespace CaptureNET.Dissectors.SDP
{
    public class SDPOrigin
    {
        private readonly string _username;
        public string Username => _username;

        private readonly string _sessionId;
        public string SessionID => _sessionId;

        private readonly string _sessionVersion;
        public string SessionVersion => _sessionVersion;

        private readonly IPAddress _ipAddress;
        public IPAddress IPAddress => _ipAddress;

        private readonly string _value;
        public string Value => _value;

        public SDPOrigin(in string value)
        {
            _value = value;

            string[] parts = value.Split(' ');
            _username = parts[0];
            _sessionId = parts[1];
            _sessionVersion = parts[2];
            if (parts[3] == "IN" && (parts[4] == "IP4" || parts[4] == "IP6"))
                _ipAddress = IPAddress.Parse(parts[5]);
            else
                _ipAddress = null;
        }
    }
}
