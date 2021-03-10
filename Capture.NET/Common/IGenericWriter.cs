using System.IO;

namespace CaptureNET.Common
{
    public interface IGenericWriter
    {
        void Open(in string path, in FileMode mode);
        void Open(in Stream stream);
        void WriteGenericHeader();
        void Write(in IGenericPacket packet);
        void Close();
    }
}
