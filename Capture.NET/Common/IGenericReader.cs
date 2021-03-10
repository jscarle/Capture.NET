using System.IO;
using System.Threading;
using CaptureNET.Common.Helpers;

namespace CaptureNET.Common
{
    public interface IGenericReader
    {
        event GenericPacketEventHandler GenericPacketRead;
        void Open(in string path, in CancellationToken cancellationToken);
        void Open(in Stream stream, in CancellationToken cancellationToken);
        void Close();
    }
}
