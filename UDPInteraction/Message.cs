using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UDPInteraction
{
    public class Message
    {

        public Message(string header)
        {
            this.Header = header;
        }

        public Message (string header, string data)
        {
            this.Header = header;
            this.Data = data;
        }

        public string GetJSON()
        {
            string json = JsonConvert.SerializeObject(this);

            return json;
        }

        public static Message GetMessage(string json)
        {
            Message deserializedObject = JsonConvert.DeserializeObject<Message>(json);

            return deserializedObject;
        }

        public string Header { get; set; }
        public string Data { get; set; }
        public List<object> Attatchment { get; set; }
        public IPAddress SenderIpAddress { get; set; }
    }
}
