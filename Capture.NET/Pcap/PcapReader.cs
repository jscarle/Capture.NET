using System;
using System.IO;
using System.Threading;
using CaptureNET.Common;
using CaptureNET.Common.Helpers;
using CaptureNET.Pcap.Helpers;

namespace CaptureNET.Pcap
{
    public sealed class PcapReader : IGenericReader, IDisposable
    {
        public event HeaderEventHandler HeaderRead;
        public event PacketEventHandler PacketRead;
        public event GenericPacketEventHandler GenericPacketRead;
        private PcapHeader _header;
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

            lock (_streamLock)
            {
                _stream = stream;
                _binaryReader = new BinaryReader(_stream);
                _cancellationToken = cancellationToken;
            }

            _header = new PcapHeader(_binaryReader);
            HeaderRead?.Invoke(_header);
        }

        public void Read()
        {
            while (_binaryReader.BaseStream.Position < _binaryReader.BaseStream.Length && !_cancellationToken.IsCancellationRequested)
            {
                PcapPacket packet;
                lock (_streamLock)
                    packet = new PcapPacket(_binaryReader, _header.NanosecondResolution);
                PacketRead?.Invoke(packet);
                GenericPacketRead?.Invoke(packet);
            }
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