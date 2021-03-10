using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using CaptureNET.Dissectors.UDP;

namespace CaptureNET.Dissectors.SIP.Helpers
{
    public static class Extensions
    {
        public static bool IsSIPPayload(this UDPDatagram udpDatagram)
        {
            return IsSIPPayload(udpDatagram.Payload);
        }

        private static bool IsSIPPayload(in ReadOnlySpan<byte> payload)
        {
            int crIndex = payload.IndexOf((byte)0x0D);
            int lfIndex = payload.IndexOf((byte)0x0A);
            if (crIndex <= 0 || lfIndex <= 0 || lfIndex != crIndex + 1)
                return false;

            if (Encoding.UTF8.GetString(payload.Slice(0, crIndex)).Contains("SIP/2.0"))
                return true;

            return false;
        }

        public static bool IsSIPResponse(this UDPDatagram udpDatagram)
        {
            return IsSIPResponse(udpDatagram.Payload);
        }

        private static bool IsSIPResponse(in ReadOnlySpan<byte> payload)
        {
            if (Encoding.UTF8.GetString(payload.Slice(0, 7)) == "SIP/2.0")
                return true;

            return false;
        }

        private static readonly ImmutableDictionary<string, string> ProperCaseHeaders = new Dictionary<string, string> { { "accept", "Accept" }, { "accept-encoding", "Accept-Encoding" }, { "accept-language", "Accept-Language" }, { "alert-info", "Alert-Info" }, { "allow", "Allow" }, { "authentication-info", "Authentication-Info" }, { "authorization", "Authorization" }, { "call-id", "Call-ID" }, { "call-info", "Call-Info" }, { "contact", "Contact" }, { "content-disposition", "Content-Disposition" }, { "content-encoding", "Content-Encoding" }, { "content-language", "Content-Language" }, { "content-length", "Content-Length" }, { "content-type", "Content-Type" }, { "cseq", "CSeq" }, { "date", "Date" }, { "error-info", "Error-Info" }, { "expires", "Expires" }, { "from", "From" }, { "in-reply-to", "In-Reply-To" }, { "max-forwards", "Max-Forwards" }, { "min-expires", "Min-Expires" }, { "mime-version", "MIME-Version" }, { "organization", "Organization" }, { "priority", "Priority" }, { "proxy-authenticate", "Proxy-Authenticate" }, { "proxy-authorization", "Proxy-Authorization" }, { "proxy-require", "Proxy-Require" }, { "record-route", "Record-Route" }, { "reply-to", "Reply-To" }, { "require", "Require" }, { "retry-after", "Retry-After" }, { "route", "Route" }, { "server", "Server" }, { "subject", "Subject" }, { "supported", "Supported" }, { "timestamp", "Timestamp" }, { "to", "To" }, { "unsupported", "Unsupported" }, { "user-agent", "User-Agent" }, { "via", "Via" }, { "warning", "Warning" }, { "www-authenticate", "WWW-Authenticate" } }.ToImmutableDictionary();
        private static readonly ImmutableDictionary<string, string> CompactFormHeaders = new Dictionary<string, string> { { "i", "Call-ID" }, { "m", "Contact" }, { "e", "Content-Encoding" }, { "l", "Content-Length" }, { "c", "Content-Type" }, { "f", "From" }, { "s", "Subject" }, { "k", "Supported" }, { "t", "To" } }.ToImmutableDictionary();

        public static string ToProperCaseHeader(this string header)
        {
            string lowerCaseHeader = header.ToLower();
            return ProperCaseHeaders.ContainsKey(lowerCaseHeader) ? ProperCaseHeaders[lowerCaseHeader] : header;
        }

        public static string ToLongFormHeader(this string header)
        {
            string lowerCaseHeader = header.ToLower();
            return CompactFormHeaders.ContainsKey(lowerCaseHeader) ? CompactFormHeaders[lowerCaseHeader] : header;
        }

        public static ImmutableDictionary<string, string> ToParameters(this string str, char delimiter)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new Dictionary<string, string>().ToImmutableDictionary();

            if (!str.Contains(delimiter))
            {
                (string key, string value) = ToKeyValuePair(str);
                return new Dictionary<string, string> { { key, value } }.ToImmutableDictionary();
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            int parameterStartIndex = 0;
            int quoteStartIndex = 0;
            bool insideQuotes = false;
            while (quoteStartIndex != -1 && quoteStartIndex < str.Length)
            {
                quoteStartIndex = str.IndexOfAny(new char[] { delimiter, '"' }, quoteStartIndex);
                if (quoteStartIndex == -1)
                    continue;
                if (quoteStartIndex <= parameterStartIndex && str[quoteStartIndex] == delimiter)
                {
                    insideQuotes = false;
                    quoteStartIndex++;
                    parameterStartIndex = quoteStartIndex;
                }
                else if (str[quoteStartIndex] == '"')
                {
                    switch (insideQuotes)
                    {
                        case true when quoteStartIndex > 0 && str[quoteStartIndex - 1] != '\\':
                            insideQuotes = false;
                            break;
                        case false:
                            insideQuotes = true;
                            break;
                    }
                    quoteStartIndex++;
                }
                else
                {
                    if (!insideQuotes)
                    {
                        (string key, string value) = ToKeyValuePair(str[parameterStartIndex..quoteStartIndex]);
                        parameters.Add(key, value);
                        quoteStartIndex++;
                        parameterStartIndex = quoteStartIndex;
                    }
                    else
                    {
                        quoteStartIndex++;
                    }
                }
            }

            if (parameterStartIndex < str.Length)
            {
                (string key, string value) = ToKeyValuePair(str[parameterStartIndex..]);
                if (!parameters.ContainsKey(key))
                    parameters.Add(key, value);
            }

            return parameters.ToImmutableDictionary();
        }

        private static KeyValuePair<string, string> ToKeyValuePair(string str)
        {
            int separatorIndex = str.IndexOf('=');
            if (separatorIndex != -1)
            {
                string keyName = str.Substring(0, separatorIndex).Trim().ToLower();
                return new KeyValuePair<string, string>(keyName, str[(separatorIndex + 1)..].Trim());
            }
            else
            {
                return new KeyValuePair<string, string>(str, null);
            }
        }
    }
}
