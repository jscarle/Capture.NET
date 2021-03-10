using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CaptureNET.Dissectors.SIP.Helpers;

namespace CaptureNET.Dissectors.SIP
{
    public sealed class SIPUser
    {
        public string Name => _name;
        private readonly string _name;

        public SIPURI URI => _uri;
        private readonly SIPURI _uri;

        public ImmutableDictionary<string, string> Parameters => _parameters;
        private readonly ImmutableDictionary<string, string> _parameters;

        private readonly string _value;

        public SIPUser(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{nameof(value)} cannot be null or empty.");

            string uri = value.Trim();
            _value = value;

            int uriStart = uri.IndexOf('<');
            if (uriStart == -1)
            {
                _name = "";
                int parametersIndex = uri.IndexOf(';');
                if (parametersIndex != -1)
                {
                    _uri = new SIPURI(uri.Substring(0, parametersIndex));
                    _parameters = uri[(parametersIndex + 1)..].ToParameters(';');
                }
                else
                {
                    _uri = new SIPURI(uri);
                    _parameters = new Dictionary<string, string>().ToImmutableDictionary();
                }
            }
            else
            {
                if (uriStart > 0)
                {
                    _name = uri.Substring(0, uriStart).Trim(new char[] { ' ', '"' });
                    uri = uri[uriStart..];
                }
                int uriEnd = uri.IndexOf('>');
                if (uriEnd == -1)
                    throw new Exception("Missing right quote.");
                _uri = new SIPURI(uri[1..uriEnd]);
                _parameters = uri[(uriEnd + 1)..].ToParameters(';');
            }
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
