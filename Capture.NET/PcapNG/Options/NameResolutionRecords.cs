using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CaptureNET.PcapNG.Options.Helpers;

namespace CaptureNET.PcapNG.Options
{
    public sealed class NameResolutionRecords : Options, IList<NameResolutionRecord>
    {
        private readonly List<NameResolutionRecord> _records = new List<NameResolutionRecord>();

        public NameResolutionRecords(in List<NameResolutionRecord> nameResolutionRecords)
        {
            if (nameResolutionRecords == null)
                throw new ArgumentNullException($"{nameof(nameResolutionRecords)} cannot be null.");

            _records = nameResolutionRecords;
        }

        public NameResolutionRecords(in ReadOnlySpan<byte> bytes, out int finalOffset)
        {
            if (bytes == null)
                throw new ArgumentNullException($"{nameof(bytes)} cannot be null.");

            foreach ((ushort key, byte[] value) in ReadOptions(bytes, out finalOffset))
            {
                switch (key)
                {
                    case (ushort)NameResolutionRecordCodes.IPv4Record:
                        {
                            if (value.Length >= 4)
                            {
                                byte[] ipv4AddressBytes = value[..4];
                                byte[] descriptionBytes = value[4..];
                                IPAddress ipAddress = new IPAddress(ipv4AddressBytes);
                                string description = Encoding.UTF8.GetString(descriptionBytes);
                                NameResolutionRecord nameResolutionRecord = new NameResolutionRecord(ipAddress, description);
                                _records.Add(nameResolutionRecord);
                            }
                            break;
                        }
                    case (ushort)NameResolutionRecordCodes.IPv6Record:
                        {
                            if (value.Length >= 16)
                            {
                                byte[] ipv6AddressBytes = value[..16];
                                byte[] descriptionBytes = value[16..];
                                IPAddress ipAddress = new IPAddress(ipv6AddressBytes);
                                string description = Encoding.UTF8.GetString(descriptionBytes);
                                NameResolutionRecord nameResolutionRecord = new NameResolutionRecord(ipAddress, description);
                                _records.Add(nameResolutionRecord);
                            }
                            break;
                        }
                    case (ushort)NameResolutionRecordCodes.EndOfRecord:
                        break;

                    default:
                        Debug.WriteLine($"Unknown Name Resolution Records Code of {key}.");
                        break;
                }
            }
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();

            foreach (NameResolutionRecord record in _records)
            {
                switch (record.IPAddress.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        {
                            List<byte> ipv4RecordBytes = new List<byte>();
                            ipv4RecordBytes.AddRange(record.IPAddress.GetAddressBytes());
                            ipv4RecordBytes.AddRange(Encoding.UTF8.GetBytes(record.Description));
                            if (ipv4RecordBytes.Count <= UInt16.MaxValue)
                                bytes.AddRange(ConvertOptionFieldToBytes((ushort)NameResolutionRecordCodes.IPv4Record, ipv4RecordBytes.ToArray()));
                        }
                        break;

                    case AddressFamily.InterNetworkV6:
                        {
                            List<byte> ipv6RecordBytes = new List<byte>();
                            ipv6RecordBytes.AddRange(record.IPAddress.GetAddressBytes());
                            ipv6RecordBytes.AddRange(Encoding.UTF8.GetBytes(record.Description));
                            if (ipv6RecordBytes.Count <= UInt16.MaxValue)
                                bytes.AddRange(ConvertOptionFieldToBytes((ushort)NameResolutionRecordCodes.IPv6Record, ipv6RecordBytes.ToArray()));
                        }
                        break;
                }
            }

            if (bytes.Count > 0)
                bytes.AddRange(ConvertOptionFieldToBytes((ushort)NameResolutionRecordCodes.EndOfRecord, Array.Empty<byte>()));

            return bytes.ToArray();
        }

        public NameResolutionRecord this[int index]
        {
            get => _records[index];
            set => _records[index] = value;
        }
        public int Count => _records.Count;
        public bool IsReadOnly => false;
        public void Add(NameResolutionRecord item) => _records.Add(item);
        public void Clear() => _records.Clear();
        public bool Contains(NameResolutionRecord item) => _records.Contains(item);
        public void CopyTo(NameResolutionRecord[] array, int arrayIndex) => _records.CopyTo(array, arrayIndex);
        public IEnumerator<NameResolutionRecord> GetEnumerator() => _records.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _records.GetEnumerator();
        public int IndexOf(NameResolutionRecord item) => _records.IndexOf(item);
        public void Insert(int index, NameResolutionRecord item) => _records.Insert(index, item);
        public bool Remove(NameResolutionRecord item) => _records.Remove(item);
        public void RemoveAt(int index) => _records.RemoveAt(index);
    }
}