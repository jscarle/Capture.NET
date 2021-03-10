using System.Collections.Immutable;

namespace CaptureNET.Dissectors.SDP
{
    // RFC: https://tools.ietf.org/html/rfc4566
    public sealed class SDPSessionDescription
    {
        private readonly byte _version;
        public byte Version => _version;

        private readonly SDPOrigin _origin;
        public SDPOrigin Origin => _origin;

        private readonly string _sessionName;
        public string SessionName => _sessionName;

        private readonly string _sessionInformation;
        public string SessionInformation => _sessionInformation;

        private readonly string _uri;
        public string URI => _uri;

        private readonly ImmutableList<string> _emailAddresses = ImmutableList<string>.Empty;
        public ImmutableList<string> EmailAddresses => _emailAddresses;

        private readonly ImmutableList<string> _phoneNumbers = ImmutableList<string>.Empty;
        public ImmutableList<string> PhoneNumbers => _phoneNumbers;

        private readonly SDPConnection _connection;
        public SDPConnection Connection => _connection;

        private readonly string _bandwidth;
        public string Bandwidth => _bandwidth;

        private readonly ImmutableList<SDPTimeDescription> _times = ImmutableList<SDPTimeDescription>.Empty;
        public ImmutableList<SDPTimeDescription> Times => _times;

        private readonly ImmutableList<string> _timeZones = ImmutableList<string>.Empty;
        public ImmutableList<string> TimeZones => _timeZones;

        private readonly ImmutableList<string> _encryptionKeys = ImmutableList<string>.Empty;
        public ImmutableList<string> EncryptionKeys => _encryptionKeys;

        private readonly ImmutableDictionary<string, ImmutableList<string>> _attributes = ImmutableDictionary<string, ImmutableList<string>>.Empty;
        public ImmutableDictionary<string, ImmutableList<string>> Attributes => _attributes;

        private readonly ImmutableList<SDPMediaDescription> _medias = ImmutableList<SDPMediaDescription>.Empty;
        public ImmutableList<SDPMediaDescription> Medias => _medias;

        public SDPSessionDescription(in string body)
        {
            bool mediaFields = false;
            string[] lines = body.Split("\r\n");
            foreach (string line in lines)
                if (line.Length > 3)
                    switch (line[0])
                    {
                        case 'v':
                            _version = byte.Parse(line[2..]);
                            break;
                        case 'o':
                            _origin = new SDPOrigin(line[2..]);
                            break;
                        case 's':
                            _sessionName = line[2..];
                            break;
                        case 'i':
                            if (mediaFields)
                                _medias[^1].SetMediaInformation(line[2..]);
                            else
                                _sessionInformation = line[2..];
                            break;
                        case 'u':
                            _uri = line[2..];
                            break;
                        case 'e':
                            _emailAddresses = _emailAddresses.Add(line[2..]);
                            break;
                        case 'p':
                            _phoneNumbers = _phoneNumbers.Add(line[2..]);
                            break;
                        case 'c':
                            if (mediaFields)
                                _medias[^1].SetConnection(new SDPConnection(line[2..]));
                            else
                                _connection = new SDPConnection(line[2..]);
                            break;
                        case 'b':
                            if (mediaFields)
                                _medias[^1].SetBandwidth(line[2..]);
                            else
                                _bandwidth = line[2..];
                            break;
                        case 't':
                            _times = _times.Add(new SDPTimeDescription(line[2..]));
                            break;
                        case 'r':
                            _times[^1].AddRepeat(line[2..]);
                            break;
                        case 'z':
                            _timeZones = _timeZones.Add(line[2..]);
                            break;
                        case 'k':
                            if (mediaFields)
                                _medias[^1].AddEncryptionKey(line[2..]);
                            else
                                _encryptionKeys = _encryptionKeys.Add(line[2..]);
                            break;
                        case 'a':
                            if (mediaFields)
                            {
                                _medias[^1].AddAttribute(line[2..]);
                            }
                            else
                            {
                                string[] attribute = line[2..].Split(':');

                                if (!_attributes.ContainsKey(attribute[0]))
                                    _attributes = _attributes.Add(attribute[0], ImmutableList<string>.Empty);
                                if (attribute.Length == 2)
                                    _attributes = _attributes.SetItem(attribute[0], _attributes[attribute[0]].Add(attribute[1]));
                            }
                            break;
                        case 'm':
                            mediaFields = true;
                            _medias = _medias.Add(new SDPMediaDescription(line[2..]));
                            break;
                    }

        }
    }
}
