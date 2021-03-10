using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CaptureNET.Common;
using CaptureNET.Common.Helpers;
using CaptureNET.Dissectors.Ethernet;
using CaptureNET.Dissectors.Ethernet.Helpers;
using CaptureNET.Dissectors.ICMP;
using CaptureNET.Dissectors.ICMP.Helpers;
using CaptureNET.Dissectors.IP;
using CaptureNET.Dissectors.IP.IPv4;
using CaptureNET.Dissectors.IP.IPv4.Helpers;
using CaptureNET.Dissectors.RTP;
using CaptureNET.Dissectors.RTP.Helpers;
using CaptureNET.Dissectors.SIP;
using CaptureNET.Dissectors.SIP.Helpers;
using CaptureNET.Dissectors.UDP;
using CaptureNET.Dissectors.UDP.Helpers;
using CaptureNET.PcapNG.Blocks;
using CaptureNET.PcapNG.Helpers;

namespace CaptureNET.PcapNG
{
    public sealed class PcapNGReader : IGenericReader, IDisposable
    {
        public event BlockEventHandler BlockRead;
        public event SectionHeaderBlockEventHandler SectionHeaderBlockRead;
        public event InterfaceDescriptionBlockEventHandler InterfaceDescriptionBlockRead;
        public event EnhancedPacketBlockEventHandler EnhancedPacketBlockRead;
        [Obsolete("Use EnhancedPacketBlock instead.", false)]
        public event PacketBlockEventHandler PacketBlockRead;
        public event SimplePacketEventHandler SimplePacketBlockRead;
        public event NameResolutionEventHandler NameResolutionBlockRead;
        public event InterfaceStatisticsEventHandler InterfaceStatisticsBlockRead;
        public event GenericPacketEventHandler GenericPacketRead;
        public event EthernetFrameEventHandler EthernetFrameDissected;
        public event IPv4PacketEventHandler IPv4PacketDissected;
        public event ICMPMessageEventHandler ICMPMessageDissected;
        public event UDPDatagramEventHandler UDPDatagramDissected;
        public event SIPMessageEventHandler SIPMessageDissected;
        public event RTPPacketEventHandler RTPPacketDissected;

        private readonly Dictionary<IPv4FragmentHash, SortedDictionary<ushort, IPv4Packet>> _ipv4Fragments = new Dictionary<IPv4FragmentHash, SortedDictionary<ushort, IPv4Packet>>();
        private readonly Dictionary<IPv4FragmentHash, byte[]> _ipv4Payloads = new Dictionary<IPv4FragmentHash, byte[]>();
        private readonly ConcurrentQueue<Block> _dissectionQueue = new ConcurrentQueue<Block>();
        private Thread _dissectionThread;
        private readonly Barrier _dissectionBarrier = new Barrier(2);
        private Thread _readingThread;
        private readonly Barrier _readingBarrier = new Barrier(2);
        private BinaryReader _binaryReader;
        private Stream _stream;
        private readonly object _streamLock = new object();
        private CancellationToken _cancellationToken;
        private bool _disposed;

        public void Open(in string filename, in CancellationToken cancellationToken = new CancellationToken())
        {
            if (String.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException($"{nameof(filename)} cannot be null or empty.");
            if (!File.Exists(filename))
                throw new FileNotFoundException($"File does not exist.");

            lock (_streamLock)
            {
                _stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 262144, FileOptions.SequentialScan);
                _binaryReader = new BinaryReader(_stream);
                _cancellationToken = cancellationToken;
            }
        }

        public void Open(in Stream stream, in CancellationToken cancellationToken = new CancellationToken())
        {
            if (stream == null)
                throw new ArgumentNullException($"{nameof(stream)} cannot be null.");
            if (!stream.CanRead)
                throw new Exception("Cannot read from stream.");

            lock (_streamLock)
            {
                _stream = stream;
                _binaryReader = new BinaryReader(_stream);
                _cancellationToken = cancellationToken;
            }
        }

        public void Read()
        {
#if DEBUG
            ReadBlocks();
            DissectBlocks();
#else
            _readingThread = new Thread(ReadBlocks);
            _dissectionThread = new Thread(DissectBlocks);

            _readingThread.Start();
            _dissectionThread.Start();
            
            _readingBarrier.SignalAndWait();
            _dissectionBarrier.SignalAndWait();

            _readingThread = null;
            _dissectionThread = null;
#endif
        }

        private void ReadBlocks()
        {
            SectionHeaderBlock currentSection = null;
            List<InterfaceDescriptionBlock> currentInterfaces = null;

            while (_binaryReader.BaseStream.Position < _binaryReader.BaseStream.Length && !_cancellationToken.IsCancellationRequested)
            {
                Block block;
                lock (_streamLock)
                    block = ReadNextBlock(_binaryReader);
                BlockRead?.Invoke(block);
                switch (block)
                {
                    case SectionHeaderBlock sectionHeader:
                        SectionHeaderBlockRead?.Invoke(sectionHeader);
                        currentSection = sectionHeader;
                        currentInterfaces = new List<InterfaceDescriptionBlock>();
                        break;

                    case InterfaceDescriptionBlock interfaceDescription:
                        InterfaceDescriptionBlockRead?.Invoke(interfaceDescription);
                        if (currentSection != null)
                            currentInterfaces.Add(interfaceDescription);
                        break;

                    case EnhancedPacketBlock enhancedPacketBlock:
                        EnhancedPacketBlockRead?.Invoke(enhancedPacketBlock);
                        GenericPacketRead?.Invoke(enhancedPacketBlock);
                        if (currentInterfaces != null && currentInterfaces[enhancedPacketBlock.InterfaceID].LinkType == LinkTypes.Ethernet)
                            _dissectionQueue.Enqueue(enhancedPacketBlock);
                        break;

#pragma warning disable CS0618
                    case PacketBlock packetBlock:
                        PacketBlockRead?.Invoke(packetBlock);
                        GenericPacketRead?.Invoke(packetBlock);
                        if (currentInterfaces != null && currentInterfaces[packetBlock.InterfaceID].LinkType == LinkTypes.Ethernet)
                            _dissectionQueue.Enqueue(packetBlock);
                        break;
#pragma warning restore CS0618

                    case SimplePacketBlock simplePacketBlock:
                        SimplePacketBlockRead?.Invoke(simplePacketBlock);
                        GenericPacketRead?.Invoke(simplePacketBlock);
                        if (currentInterfaces != null && currentInterfaces[0].LinkType == LinkTypes.Ethernet)
                            _dissectionQueue.Enqueue(simplePacketBlock);
                        break;

                    case NameResolutionBlock nameResolutionBlock:
                        NameResolutionBlockRead?.Invoke(nameResolutionBlock);
                        break;

                    case InterfaceStatisticsBlock interfaceStatisticsBlock:
                        InterfaceStatisticsBlockRead?.Invoke(interfaceStatisticsBlock);
                        break;

                    default:
                        break;
                }
            }
#if !DEBUG
            _readingBarrier.SignalAndWait();
#endif
        }

        private static Block ReadNextBlock(in BinaryReader binaryReader)
        {
            try
            {
                uint blockType = binaryReader.ReadUInt32();
#if DEBUG
            if (!Enum.IsDefined(typeof(BlockType), blockType))
                throw new NotImplementedException($"Unknown Block Type of 0x{blockType:x8}.");
#endif
                switch ((BlockType) blockType)
                {
                    case BlockType.SectionHeader:
                        return new SectionHeaderBlock(binaryReader);
                    case BlockType.InterfaceDescription:
                        return new InterfaceDescriptionBlock(binaryReader);
                    case BlockType.EnhancedPacket:
                        return new EnhancedPacketBlock(binaryReader);
#pragma warning disable CS0618
                    case BlockType.Packet:
                        return new PacketBlock(binaryReader);
#pragma warning restore CS0618
                    case BlockType.SimplePacket:
                        return new SimplePacketBlock(binaryReader);
                    case BlockType.NameResolution:
                        return new NameResolutionBlock(binaryReader);
                    case BlockType.InterfaceStatistics:
                        return new InterfaceStatisticsBlock(binaryReader);
                    default:
                        return null;
                }
            }
            catch (EndOfStreamException)
            {
                Debug.WriteLine("EndOfStreamException while reading next Block.");
                return null;
            }
        }

        private void DissectBlocks()
        {
            while ((
#if !DEBUG
                _readingBarrier.CurrentPhaseNumber == 0 || 
#endif
                !_dissectionQueue.IsEmpty) && !_cancellationToken.IsCancellationRequested)
                if (_dissectionQueue.TryDequeue(out Block block))
                    DissectNextBlock(block);
#if !DEBUG
            _dissectionBarrier.SignalAndWait();
#endif
        }

        private void DissectNextBlock(in Block block)
        {
            EthernetFrame ethernetFrame = new EthernetFrame(block);
            EthernetFrameDissected?.Invoke(ethernetFrame);

            if (ethernetFrame.EtherType != EtherType.IPv4)
                return;

            IPv4Packet ipv4Packet = new IPv4Packet(ethernetFrame);
            IPv4PacketDissected?.Invoke(ipv4Packet);

            switch (ipv4Packet.Protocol)
            {
                case IPProtocol.ICMP:
                    ICMPMessage icmpMessage = new ICMPMessage(ipv4Packet);
                    ICMPMessageDissected?.Invoke(icmpMessage);
                    break;

                case IPProtocol.UDP:
                    UDPDatagram udpDatagram = new UDPDatagram(ipv4Packet);
                    UDPDatagramDissected?.Invoke(udpDatagram);
                    if (udpDatagram.IsSIPPayload())
                    {
                        SIPMessage sipMessage;
                        if (udpDatagram.IsSIPResponse())
                            sipMessage = new SIPResponse(udpDatagram);
                        else
                            sipMessage = new SIPRequest(udpDatagram);
                        SIPMessageDissected?.Invoke(sipMessage);
                    }
                    else if (udpDatagram.IsLikelyRTPPayload())
                    {
                        RTPPacket rtpPacket = new RTPPacket(udpDatagram);
                        RTPPacketDissected?.Invoke(rtpPacket);
                    }
                    break;
            }
            /*
            if (ipv4Packet.IsFragment())
            {
                SaveFragment(ipv4Packet);
                if (ReconstructPayload())
                    Thread.Sleep(10);
                //Debug.WriteLine("Reconstructed payload.");
            }
            */
        }

        private void SaveFragment(IPv4Packet ipv4Packet)
        {
            IPv4FragmentHash ipv4FragmentHash = new IPv4FragmentHash(ipv4Packet.SourceIPAddress.GetAddressBytes(), ipv4Packet.DestinationIPAddress.GetAddressBytes(), ipv4Packet.ID);

            if (!_ipv4Fragments.ContainsKey(ipv4FragmentHash))
                _ipv4Fragments.Add(ipv4FragmentHash, new SortedDictionary<ushort, IPv4Packet>());

            if (!_ipv4Fragments[ipv4FragmentHash].ContainsKey(ipv4Packet.FragmentOffset))
                _ipv4Fragments[ipv4FragmentHash].Add(ipv4Packet.FragmentOffset, ipv4Packet);
        }

        private bool ReconstructPayload()
        {
            if (_ipv4Fragments.Count < 2)
                return false;

            Dictionary<IPv4FragmentHash, SortedDictionary<ushort, IPv4Packet>>.KeyCollection ipv4FragmentHashes = _ipv4Fragments.Keys;
            foreach (IPv4FragmentHash ipv4FragmentHash in ipv4FragmentHashes)
            {
                SortedDictionary<ushort, IPv4Packet> fragments = _ipv4Fragments[ipv4FragmentHash];
                if (!fragments.ContainsKey(0))
                    continue;

                List<byte> payload = new List<byte>();

                bool areMoreFragments = true;
                bool foundLastFragment = false;
                ushort nextOffset = 0;
                while (!foundLastFragment && areMoreFragments)
                {
                    IPv4Packet nextFrame = fragments[nextOffset];
                    foundLastFragment = !nextFrame.Flags.MoreFragments;
                    payload.AddRange(nextFrame.Payload.ToArray());
                    nextOffset = (ushort)(nextOffset + nextFrame.TotalLength - (nextFrame.IHL * 4));
                    if (!fragments.ContainsKey(nextOffset))
                        areMoreFragments = false;
                }

                if (foundLastFragment)
                {
                    _ipv4Payloads.Add(ipv4FragmentHash, payload.ToArray());
                    _ipv4Fragments.Remove(ipv4FragmentHash);
                    return true;
                }
            }

            return false;
        }

        public void Close()
        {
            lock (_streamLock)
            {
                _binaryReader?.Close();
                _stream?.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(in bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Close();
            }
            _disposed = true;
        }
    }
}