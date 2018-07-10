using System.Net;

namespace UDPInteraction
{
    public class ByteBagElement
    {
        public ByteBagElement(byte[] bytes, IPAddress address)
        {
            Bytes = bytes;
            SenderAddress = address;
        }

        public byte[] Bytes { get; private set; }
        public IPAddress SenderAddress { get; private set; }
    }
}
