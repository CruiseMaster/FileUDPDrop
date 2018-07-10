using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace UDPInteraction
{
    public class FileInstance
    {
        public FileInstance(string filepath = null, long size = 0, byte[] md5 = null, int UDPPort = 0)
        {
            FilePath = filepath;
            Size = size;
            CRC = md5;
            this.UDPPort = UDPPort;
        }

        public string FilePath { get; set; }

        public long Size { get; set; }

        public byte[] CRC { get; private set; }

        public int UDPPort { get; set; }

        public byte[] ComputeHash()
        {
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (FileStream stream = new FileStream(FilePath, FileMode.Open))
                    {
                        CRC = md5.ComputeHash(stream);
                        return CRC;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}
