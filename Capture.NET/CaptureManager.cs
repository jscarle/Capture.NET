using System;
using System.IO;
using CaptureNET.Common;
using CaptureNET.Pcap;
using CaptureNET.PcapNG;
using CaptureNET.PcapNG.Blocks;

namespace CaptureNET
{
    public static class CaptureManager
    {
        public static IGenericReader GetReader(in string path)
        {
            uint magicNumber;

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                using BinaryReader binaryReader = new BinaryReader(stream);
                if (binaryReader.BaseStream.Length < 12)
                    throw new ArgumentException("File does not contain sufficient data.");

                magicNumber = binaryReader.ReadUInt32();
                if (magicNumber == (uint)BlockType.SectionHeader)
                {
                    binaryReader.ReadUInt32();
                    magicNumber = binaryReader.ReadUInt32();
                }
            }

            switch (magicNumber)
            {
                case (uint)Pcap.MagicNumbers.MicrosecondIdentical:
                case (uint)Pcap.MagicNumbers.MicrosecondSwapped:
                case (uint)Pcap.MagicNumbers.NanosecondSwapped:
                case (uint)Pcap.MagicNumbers.NanosecondIdentical:
                {
                    IGenericReader reader = new PcapReader();
                    return reader;
                }
                case (uint)PcapNG.Blocks.MagicNumbers.Identical:
                case (uint)PcapNG.Blocks.MagicNumbers.Swapped:
                {
                    IGenericReader reader = new PcapNGReader();
                    return reader;
                }
                default:
                    throw new ArgumentException("File is not in a standard PCAP or PCAPNG format.");
            }
        }
    }
}