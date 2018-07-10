using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            byteBag = new ConcurrentQueue<ByteBagElement>();
            this.abort = false;
            this.TypeOfServer = serverType;
        }

        public event EventHandler<long> BytesReceivedNotification;
        public event EventHandler<bool> FileTransmitStopped;

        public void StartServer()
        {
            Task.Run(() =>
            {
                UdpClient client = new UdpClient(Port);
                while(!abort)
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
                    byte[] receivedBytes = client.Receive(ref remoteEndPoint);
                    if (remoteEndPoint.Address.Equals(IPAddress.Loopback))
                        continue;

                    ByteBagElement receivedElement = new ByteBagElement(receivedBytes, remoteEndPoint.Address);
                    byteBag.Enqueue(receivedElement);
                }

                client.Close();
            });

            AutomaticByteBagEmptynator();
        }

        public void AutomaticByteBagEmptynator(FileInstance instance = null)
        {
            Task.Run(() =>
            {
                if (TypeOfServer == ServerType.NEGOTIATION)
                    ReceiveNegotiationMessages();
                else if (TypeOfServer == ServerType.FILERECEIVER)
                    HandleFileReceiving(instance);
            });
        }

        private void HandleFileReceiving(FileInstance instance)
        {
            if (instance == null)
            {
                abort = true;
                return;
            }

            try
            {
                using (FileStream stream = new FileStream(instance.FilePath, FileMode.Create))
                {
                    long awaitedLength = instance.Size;
                    long actualLength = 0;
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    do
                    {
                        byteBag.TryDequeue(out ByteBagElement currentElement);
                        byte[] currentArray = currentElement?.Bytes;
                        if (currentArray == null || currentArray.Length == 0)
                            continue;

                        actualLength += currentArray.Length;
                        stream.Write(currentArray, 0, currentArray.Length);
                        BytesReceivedNotification?.Invoke(this, actualLength);
                        watch.Restart();
                    } while (actualLength < awaitedLength && watch.ElapsedMilliseconds < 10000);

                    FileTransmitStopped?.Invoke(this, actualLength == awaitedLength);
                    abort = true;
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                abort = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                abort = true;
            }

        }

        private void ReceiveNegotiationMessages()
        {
            while (!abort)
            {
                Thread.Sleep(500);
                byteBag.TryDequeue(out ByteBagElement currentElement);
                byte[] currentArray = currentElement?.Bytes;

                if (currentArray == null || currentArray.Length == 0)
                    continue;

                string messageString = Encoding.BigEndianUnicode.GetString(currentArray);

                Message message = Message.GetMessage(messageString);
                message.SenderIpAddress = currentElement.SenderAddress;
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

        public delegate void ChatDelegate(ChatMessage cm, Message msg);

        public delegate void FileDelegate(FileInstance inst, Message msg);
        public event ChatDelegate ChatMessageIncommingPropagation;
        public event FileDelegate FileAnnouncementPropagation;
        public event EventHandler<Message> MessageIncommingPropagation;

        public void PropagateFileOffer(Message message)
        {
            if (message.Attatchment[0] == null || message.Attatchment[0].GetType() != typeof(FileInstance) || message.Attatchment[0].GetType() != typeof(DirectoryInstance))
                return;

            FileInstance instance = (FileInstance) message.Attatchment[0];
            FileAnnouncementPropagation?.Invoke(instance, message);
        }

        public void PropagateChatMessage(Message message)
        {
            if (message.Attatchment[0] == null || message.Attatchment[0].GetType() != typeof(ChatMessage))
                return;

            ChatMessage instance = (ChatMessage) message.Attatchment[0];
            ChatMessageIncommingPropagation?.Invoke(instance, message);
        }

        public void PropagateMessage(Message message)
        {
            MessageIncommingPropagation?.Invoke(this, message);
        }

        public void Close()
        {
            this.abort = true;
        }

        private readonly ConcurrentQueue<ByteBagElement> byteBag;

        public enum ServerType
        {
            NEGOTIATION,
            FILERECEIVER
        }
    }
}
