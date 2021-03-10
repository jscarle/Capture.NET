using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CaptureNET.Dissectors.SIP.Helpers;

namespace CaptureNET.Dissectors.SIP
{
    public sealed class SIPURI
    {
        public string Scheme => _scheme;
        private readonly string _scheme;

        public string User => _user;
        private readonly string _user;

        public string Host => _host;
        private readonly string _host;

        public ushort Port => _port;
        private readonly ushort _port;

        public string Protocol => _protocol;
        private readonly string _protocol;

        public ImmutableDictionary<string, string> Parameters => _parameters;
        private ImmutableDictionary<string, string> _parameters;

        public ImmutableDictionary<string, string> Headers => _headers;
        private ImmutableDictionary<string, string> _headers;

        private readonly string _value;

        public SIPURI(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{nameof(value)} cannot be null or empty.");

            string uri = value.Trim();
            _value = uri;

            int schemeIndex = uri.IndexOf(':');
            _scheme = uri.Substring(0, schemeIndex).ToLower();

            string hostPart = uri[(schemeIndex + 1)..];
            int userIndex = hostPart.IndexOf('@');
            int parametersIndex;
            if (userIndex != -1)
                parametersIndex = hostPart.IndexOfAny(new char[] { ';', '?' }, userIndex);
            else
                parametersIndex = hostPart.IndexOfAny(new char[] { ';', '?' });

            if (userIndex != -1 && parametersIndex != -1)
            {
                _user = hostPart.Substring(0, userIndex).ToLower();
                _host = hostPart.Substring(userIndex + 1, parametersIndex - userIndex - 1).ToLower();
                ParseParameters(hostPart[parametersIndex..]);
            }
            else if (userIndex == -1 && parametersIndex > 0)
            {
                _user = "";
                _host = hostPart.Substring(0, parametersIndex).ToLower();
                ParseParameters(hostPart[parametersIndex..]);
            }
            else if (userIndex != -1)
            {
                _user = hostPart.Substring(0, userIndex).ToLower();
                _host = hostPart.Substring(userIndex + 1, hostPart.Length - userIndex - 1).ToLower();
                _parameters = new Dictionary<string, string>().ToImmutableDictionary();
                _headers = new Dictionary<string, string>().ToImmutableDictionary();
            }
            else
            {
                _user = "";
                _host = hostPart;
                _parameters = new Dictionary<string, string>().ToImmutableDictionary();
                _headers = new Dictionary<string, string>().ToImmutableDictionary();
            }

            if (_scheme == "sips")
                _protocol = "tls";
            else
                _protocol = _parameters.ContainsKey("transport") ? _parameters["transport"].ToLower() : "udp";

            if (_host.IndexOf(':') != _host.LastIndexOf(':'))
                while (_host.Contains(":::"))
                    _host = _host.Replace(":::", "::");

            int ipv4Index = _host.IndexOf(':');
            int ipv6Index = _host.IndexOf("]:");
            if (ipv6Index > 0)
            {
                _port = ushort.Parse(_host[(ipv6Index + 2)..]);
                _host = _host.Substring(0, ipv6Index + 1);
            }
            else if (ipv4Index > 0 && ipv4Index == _host.LastIndexOf(':'))
            {
                _port = ushort.Parse(_host[(ipv4Index + 1)..]);
                _host = _host.Substring(0, ipv4Index);
            }
            else
            {
                switch (_protocol)
                {
                    case "udp":
                    case "tcp":
                    case "sctp":
                        _port = 5060;
                        break;
                    case "tls":
                        _port = 5061;
                        break;
                }
            }
        }

        private void ParseParameters(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            int headerIndex = value.IndexOf('?');
            if (headerIndex == -1)
            {
                
                _parameters = value.ToParameters(';');
                _headers = new Dictionary<string, string>().ToImmutableDictionary();
            }
            else
            {
                _parameters = value.Substring(0, headerIndex).ToParameters(';');
                _headers = value[(headerIndex + 1)..].ToParameters('&');
            }
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
