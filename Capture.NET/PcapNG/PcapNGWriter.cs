using System;
using System.IO;
using System.Reflection;
using CaptureNET.Common;
using CaptureNET.PcapNG.Blocks;
using CaptureNET.PcapNG.Options;

namespace CaptureNET.PcapNG
{
    public sealed class PcapNGWriter : IGenericWriter, IDisposable
    {
        public long PositionInStream => _stream.Position;

        private Stream _stream;
        private BinaryWriter _binaryWriter;
        private readonly object _streamLock = new object();
        private bool _disposed;

        public void Open(in string path, in FileMode mode = FileMode.Create)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException($"{nameof(path)} cannot be null or empty.");
            //if (File.Exists(path))
            //throw new IOException($"File exists.");

            lock (_streamLock)
            {
                _stream = new FileStream(path, mode, FileAccess.Write, FileShare.None, 262144, FileOptions.None);
                _binaryWriter = new BinaryWriter(_stream);
            }
        }

        public void Open(in Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException($"{nameof(stream)} cannot be null.");
            if (!stream.CanWrite)
                throw new Exception("Cannot write to stream.");

            lock (_streamLock)
            {
                _stream = stream;
                _binaryWriter = new BinaryWriter(_stream);
            }
        }

        public void Write(in IGenericPacket packet)
        {
            Write(packet.ToBytes());
        }

        public void Write(in Block block)
        {
            Write(block.ToBytes());
        }

        private void Write(in byte[] data)
        {
            lock (_streamLock)
                _binaryWriter.Write(data);
        }

        public void WriteGenericHeader()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assembly = executingAssembly.GetName();
            SectionHeaderOptions sectionHeaderOptions = new SectionHeaderOptions(userApplication: $"{assembly.Name} {assembly.Version}");

            SectionHeaderBlock sectionHeaderBlock = new SectionHeaderBlock(sectionHeaderOptions);
            Write(sectionHeaderBlock);

            InterfaceDescriptionBlock interfaceDescriptionBlock = new InterfaceDescriptionBlock(LinkTypes.Ethernet, 65535, null);
            Write(interfaceDescriptionBlock);
        }

        public void Close()
        {
            lock (_streamLock)
            {
                _binaryWriter?.Close();
                _stream?.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(in bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                }
                _disposed = true;
            }
        }
    }
}