using System;
using System.Collections.Immutable;

namespace CaptureNET.Dissectors.SDP
{
    public sealed class SDPTimeDescription
    {
        private readonly ulong _start;
        public ulong Start => _start;

        private readonly ulong _stop;
        public ulong Stop => _stop;

        private ImmutableList<string> _repeat = ImmutableList<string>.Empty;
        public ImmutableList<string> Repeat => _repeat;

        public SDPTimeDescription(in string value)
        {
            string[] parts = value.Split(' ');
            _start = UInt64.Parse(parts[0]);
            _stop = UInt64.Parse(parts[1]);
        }

        internal void AddRepeat(in string value)
        {
            _repeat = _repeat.Add(value);
        }
    }
}
