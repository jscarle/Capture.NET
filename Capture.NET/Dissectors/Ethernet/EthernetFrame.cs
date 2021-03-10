using System;
using System.Buffers.Binary;
using System.Net.NetworkInformation;
using CaptureNET.Common;
using CaptureNET.Common.Helpers;
using CaptureNET.Dissectors.Ethernet.Helpers;
using CaptureNET.PcapNG.Blocks;

namespace CaptureNET.Dissectors.Ethernet
{
    public sealed class EthernetFrame
    {
        private readonly PhysicalAddress _destinationMacAddress;
        public PhysicalAddress DestinationMACAddress => _destinationMacAddress;

        private readonly PhysicalAddress _sourceMacAddress;
        public PhysicalAddress SourceMACAddress => _sourceMacAddress;

        private readonly EtherType _etherType;
        public EtherType EtherType => _etherType;

        private readonly IEEE8022LLC _llc;
        public IEEE8022LLC LLC => _llc;

        private readonly IEEE8022SNAP _snap;
        public IEEE8022SNAP SNAP => _snap;

        private readonly IEEE8021Q[] _tags;
        public IEEE8021Q[] Tags => _tags;

        private readonly int _payloadStart;
        private readonly int _payloadLength;
        public ReadOnlySpan<byte> Payload => ((IGenericPacket)_related.Block).PacketData.Slice(_payloadStart, _payloadLength);

        private readonly EthernetFrameRelated _related;
        public EthernetFrameRelated Related => _related;

        public EthernetFrame(in Block block)
            : this(((IGenericPacket)block).PacketData)
        {
            _related = new EthernetFrameRelated(block);
        }

        public EthernetFrame(in ReadOnlySpan<byte> bytes)
        {
            int offset = 0;

            _destinationMacAddress = new PhysicalAddress(bytes.Forward(ref offset, 6).ToArray());
            _sourceMacAddress = new PhysicalAddress(bytes.Forward(ref offset, 6).ToArray());

            ushort etherType = BinaryPrimitives.ReadUInt16BigEndian(bytes.Forward(ref offset, 2));
            while (etherType == (ushort)EtherType.IEEE8021Q)
            {
                if (_tags == null)
                    _tags = new IEEE8021Q[1];
                else
                    Array.Resize(ref _tags, _tags.Length + 1);
                _tags[^1] = new IEEE8021Q(bytes.Forward(ref offset, 2));
                etherType = BinaryPrimitives.ReadUInt16BigEndian(bytes.Forward(ref offset, 2));
            }

            if (etherType <= 1500)
            {
                _etherType = EtherType.IEEE8023Ethernet;

                // Does IEEE 802.3 always have an LLC header?
                _llc = new IEEE8022LLC(bytes.Forward(ref offset, 3).ToArray());
                _etherType = EtherType.IEEE8022LLC;

                if (LLC.DSAP == 0xAA && LLC.SSAP == 0xAA && LLC.Control == 0x03)
                {
                    _snap = new IEEE8022SNAP(bytes.Forward(ref offset, 5));
                    _etherType = EtherType.IEEE8022SNAP;
                }
                else
                {
                    _snap = new IEEE8022SNAP();
                }
            }
            else
            {
                _llc = new IEEE8022LLC();
                _snap = new IEEE8022SNAP();
#if DEBUG
                if (!Enum.IsDefined(typeof(EtherType), etherType))
                    throw new NotImplementedException($"Unknown EtherType of 0x{etherType:x4}.");
#endif
                _etherType = (EtherType)etherType;
            }

            _payloadStart = offset;
            _payloadLength = bytes.Length - offset;
        }
    }
}