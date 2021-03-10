using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace CaptureNET.Dissectors.SDP
{
    public sealed class SDPMediaDescription
    {
        private readonly string _media;
        public string Media => _media;

        private readonly ImmutableList<ushort> _ports = ImmutableList<ushort>.Empty;
        public ImmutableList<ushort> Ports => _ports;

        private readonly string _protocol;
        public string Protocol => _protocol;

        private readonly ImmutableList<string> _formats = ImmutableList<string>.Empty;
        public ImmutableList<string> Formats => _formats;

        private string _mediaInformation;
        public string MediaInformation => _mediaInformation;

        private SDPConnection _connection;
        public SDPConnection Connection => _connection;

        private string _bandwidth;
        public string Bandwidth => _bandwidth;

        private ImmutableList<string> _encryptionKeys = ImmutableList<string>.Empty;
        public ImmutableList<string> EncryptionKeys => _encryptionKeys;

        private ImmutableDictionary<string, ImmutableList<string>> _attributes = ImmutableDictionary<string, ImmutableList<string>>.Empty;
        public ImmutableDictionary<string, ImmutableList<string>> Attributes => _attributes;

        public SDPMediaDescription(in string value)
        {
            string[] valueParts = value.Split(' ');

            _media = valueParts[0];
            _protocol = valueParts[2];
            string[] portParts = valueParts[1].Split('/');
            if (portParts.Length == 2)
            {
                int firstPort = Int32.Parse(portParts[0]);
                int count = Int32.Parse(portParts[1]);
                int step = 1;
                if (_protocol == "RTP/AVP")
                    step = 2;
                int lastPort = firstPort + ((count - 1) * step);
                
                int nextPort = firstPort;
                while (nextPort <= lastPort)
                {
                    _ports = _ports.Add((ushort)nextPort);
                    nextPort += step;
                }
            }
            else
            {
                _ports = _ports.Add(UInt16.Parse(portParts[0]));
            }

            for (int index = 3; index < valueParts.Length; index++)
                _formats = _formats.Add(valueParts[index]);
        }

        internal void SetMediaInformation(string mediaInformation)
        {
            _mediaInformation = mediaInformation;
        }

        internal void SetConnection(SDPConnection connection)
        {
            _connection = connection;
        }

        internal void SetBandwidth(string bandwidth)
        {
            _bandwidth = bandwidth;
        }

        internal void AddEncryptionKey(string encryptionKey)
        {
            _encryptionKeys = _encryptionKeys.Add(encryptionKey);
        }

        internal void AddAttribute(string line)
        {
            string[] attribute = line.Split(':');
            if (!_attributes.ContainsKey(attribute[0]))
                _attributes = _attributes.Add(attribute[0], ImmutableList<string>.Empty);
            if (attribute.Length == 2)
                _attributes = _attributes.SetItem(attribute[0], _attributes[attribute[0]].Add(attribute[1]));
        }
    }
}
