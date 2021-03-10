using System;
using System.Collections.Immutable;
using System.Net;
using CaptureNET.Dissectors.SDP.Helpers;

namespace CaptureNET.Dissectors.SDP
{
    public sealed class SDPConnection
    {
        private readonly ImmutableList<SDPConnectionAddress> _addresses = ImmutableList<SDPConnectionAddress>.Empty;
        public ImmutableList<SDPConnectionAddress> Addresses => _addresses;

        private readonly string _value;
        public string Value => _value;

        public SDPConnection(in string value)
        {
            _value = value;

            string[] connectionParts = value.Split(' ');
            if (connectionParts[0] == "IN")
            {
                string[] addressParts = connectionParts[2].Split('/');
                switch (addressParts.Length)
                {
                    case 1:
                        _addresses = _addresses.Add(new SDPConnectionAddress(IPAddress.Parse(addressParts[0]), 0));
                        break;
                    case 2:
                        switch (connectionParts[1])
                        {
                            case "IP4":
                                _addresses = _addresses.Add(new SDPConnectionAddress(IPAddress.Parse(addressParts[0]), UInt16.Parse(addressParts[1])));
                                break;
                            case "IP6":
                                for (uint count = 0; count < UInt32.Parse(addressParts[1]); count++)
                                    _addresses = _addresses.Add(new SDPConnectionAddress(IPAddress.Parse(addressParts[0]).Plus(count), 0));
                                break;
                        }
                        break;
                    case 3:
                        for (uint count = 0; count < UInt32.Parse(addressParts[2]); count++)
                            _addresses = _addresses.Add(new SDPConnectionAddress(IPAddress.Parse(addressParts[0]).Plus(count), UInt16.Parse(addressParts[1])));
                        break;
                }
            }
        }
    }
}
