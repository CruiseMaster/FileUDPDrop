using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPInteraction
{
    public class UDPServer
    {
        public int Port { get; private set; }
        public ServerType TypeOfServer { get; private set; }
        private bool abort;

        public UDPServer(int port, ServerType serverType)
        {
            this.Port = port;
            byteBag = new ConcurrentQueue<byte[]>();
            this.abort = false;
            this.TypeOfServer = serverType;
        }

        public void StartServer()
        {
            Task.Run(() =>
            {
                UdpClient client = new UdpClient(Port);
                while(!abort)
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
                    byte[] receivedBytes = client.Receive(ref remoteEndPoint);
                    byteBag.Enqueue(receivedBytes);
                }

                client.Close();
            });
        }

        public void AutomaticByteBagEmptynator()
        {
            Task.Run(() =>
            {
                if (TypeOfServer == ServerType.NEGOTIATION)
                    ReceiveNegotiationMessages();
                else if (TypeOfServer == ServerType.FILERECEIVER)
                    HandleFileReceiving();
            });
        }

        private void HandleFileReceiving()
        {

        }

        private void ReceiveNegotiationMessages()
        {
            while (!abort)
            {
                Thread.Sleep(500);
                byteBag.TryDequeue(out byte[] currentArray);

                if (currentArray == null || currentArray.Length == 0)
                    continue;

                string messageString = Encoding.BigEndianUnicode.GetString(currentArray);

                Message message = Message.GetMessage(messageString);
                Task.Run(() => HandleNegotiationMessage(message));
            }
        }

        private void HandleNegotiationMessage (Message message)
        {
            if (message?.Header == null || message.Header.Trim().Equals(string.Empty))
                return;

            switch (message.Header)
            {
                case "FileOffer":
                    PropagateFileOffer(message);
                    break;
                case "ChatMessage":
                    PropagateChatMessage(message);
                    break;
                default:
                    PropagateMessage(message);
                    break;
            }
        }

        public event EventHandler<ChatMessage> ChatMessageIncommingPropagation;
        public event EventHandler<FileInstance> FileAnnouncementPropagation;
        public event EventHandler<Message> MessageIncommingPropagation;

        public void PropagateFileOffer(Message message)
        {
            if (message.Attatchment[0] == null || message.Attatchment[0].GetType() != typeof(FileInstance) || message.Attatchment[0].GetType() != typeof(DirectoryInstance))
                return;

            FileInstance instance = (FileInstance) message.Attatchment[0];
            FileAnnouncementPropagation?.Invoke(this, instance);
        }

        public void PropagateChatMessage(Message message)
        {
            if (message.Attatchment[0] == null || message.Attatchment[0].GetType() != typeof(ChatMessage))
                return;

            ChatMessage instance = (ChatMessage) message.Attatchment[0];
            ChatMessageIncommingPropagation?.Invoke(this, instance);
        }

        public void PropagateMessage(Message message)
        {
            MessageIncommingPropagation?.Invoke(this, message);
        }

        public void Close()
        {
            this.abort = true;
        }

        private ConcurrentQueue<byte[]> byteBag;

        public enum ServerType
        {
            NEGOTIATION,
            FILERECEIVER
        }
    }
}
