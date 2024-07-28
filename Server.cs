using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace GolgedarEngine
{
    public class Server : GameObject
    {
        public const int SERVER_PORT = 64198;

        TcpListener listener;
        SortedList<int, User> users;

        public Server() : base(canCollide: false)
        {
            users = new SortedList<int, User>();

            Task.Run(Listen);
            Task.Run(CheckInClients);

            Console.WriteLine($"The server has been stabilized.");
        }

        private async Task CheckInClients()
        {
            while (true)
            {
                foreach (var client in users.Values)
                {
                    if (!IsConnected(client.TCPClient))
                    {
                        users.Remove(client.ConnectionID);
                        Console.WriteLine($"A client has been disconnected {client}.");
                    }

                    await Task.Delay(25);
                }

                await Task.Delay(250);
            }
        }

        private async Task Listen()
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Any, SERVER_PORT));
            listener.Start();

            Console.WriteLine($"The server has been started to listening.");

            while (true)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();

                NetworkStream stream = tcpClient.GetStream();

                User client = new User(tcpClient);
                users.Add(client.ConnectionID, client);

                Task handleClient = Task.Run(() =>
                {
                    HandleClientAsync(client, stream);
                });

                Console.WriteLine($"A client has been connected {client}.");
            }
        }

        private static bool IsConnected(TcpClient client)
        {
            try
            {
                if (client != null && client.Client != null && client.Client.Connected)
                {
                    if (client.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                            return false;
                    }

                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        private void HandleClientAsync(User client, NetworkStream stream)
        {
            TcpClient tcpClient = client.TCPClient;

            using (tcpClient)
            {
                bool terminateConnection = false;
                while (tcpClient?.Connected == true && !terminateConnection)
                {
                    while (stream.CanRead && stream.DataAvailable)
                    {
                        int available = tcpClient.Available;
                        byte[] buffer = new byte[available];

                        int totalMessageLength = 0;
                        try
                        {
                            totalMessageLength = stream.Read(buffer, 0, available);
                        }
                        catch (Exception e)
                        {
                            terminateConnection = true;
                            break;
                        }

                        int readIndex = 0;
                        while (totalMessageLength - readIndex > 1)
                        {
                            char code = (char)buffer[readIndex++];
                            char dataType = (char)buffer[readIndex++];

                            object data = string.Empty;
                            switch (dataType)
                            {
                                case 'C':
                                    data = BitConverter.ToChar(buffer[readIndex..(readIndex + 2)]);
                                    readIndex += 2;
                                    if (client != null)
                                        Process(client, code, (char)data);
                                    break;

                                case 'I':
                                    data = BitConverter.ToInt32(buffer[readIndex..(readIndex + 4)]);
                                    readIndex += 4;
                                    if (client != null)
                                        Process(client, code, (int)data);
                                    break;

                                case 'F':
                                    data = BitConverter.ToSingle(buffer[readIndex..(readIndex + 4)]);
                                    readIndex += 4;
                                    if (client != null)
                                        Process(client, code, (float)data);
                                    break;

                                case 'D':
                                    data = BitConverter.ToDouble(buffer[readIndex..(readIndex + 8)]);
                                    readIndex += 8;
                                    if (client != null)
                                        Process(client, code, (double)data);
                                    break;

                                case '0':
                                    if (client != null)
                                        Process(client, code);
                                    break;

                                default:
                                    int dataLength = Convert.ToInt32(dataType.ToString());
                                    data = BitConverter.ToString(buffer[readIndex..(readIndex + dataLength)]);
                                    readIndex += dataLength;
                                    if (client != null)
                                        Process(client, code, (string)data);
                                    break;
                            }

                            //Console.WriteLine($"{client}: [{code}] {data.ToString()}");
                        }
                    }

                    if (terminateConnection)
                        Console.WriteLine($"A client has been terminated {client}.");
                }
            }
        }

        public virtual void Process(User user, int code, char data) { }
        public virtual void Process(User user, int code, int data) { }
        public virtual void Process(User user, int code, float data) { }
        public virtual void Process(User user, int code, double data) { }
        public virtual void Process(User user, int code, string data) { }
        public virtual void Process(User user, int code)
        {
            ProcessDefault(user, code);
        }
        public virtual void Connected(User user)
        {
        }

        private void ProcessDefault(User user, int code)
        {
            switch (code)
            {
                case 'Q':
                    user.Disconnect();
                    break;

                case 'S':
                    Connected(user);
                    break;
            }
        }

        public override void Draw()
        {

        }
        public override void Loop()
        {

        }
    }
}
