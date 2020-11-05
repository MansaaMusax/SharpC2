using System.Net.Sockets;

namespace Shared.Models
{
    public class HTTPStateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 65535;
        public byte[] buffer = new byte[BufferSize];
        public byte[] swapBuffer = null;
    }
}