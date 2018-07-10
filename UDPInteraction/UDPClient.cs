using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPInteraction
{
    public class UDPClient
    {
        public UDPClient(int port)
        {
            this.Port = port;
        }

        public int Port { get; private set; }
    }
}
