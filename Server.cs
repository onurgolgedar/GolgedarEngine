using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.Reflection;

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
        }

        private async Task Listen()
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Any, SERVER_PORT));

            bool isListenerStarted = false;
            while (!isListenerStarted)
            {
                try
                {
                    listener.Start();
                    isListenerStarted = true;
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"The server has failed to start.");
                    isListenerStarted = false;
                }

                await Task.Delay(1000);
            }

            Console.WriteLine($"The server has been started to listening.");

            while (true)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();

                NetworkStream stream = tcpClient.GetStream();

                User user = new User(tcpClient);
                users.Add(user.ConnectionID, user);

                Task handleClient = Task.Run(() =>
                {
                    HandleClientAsync(user, stream);
                });

                Connected(user);

                Console.WriteLine($"A client has been connected {user}.");
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
                                    int dataLength = Convert.ToInt32(buffer[readIndex]);
                                    readIndex++;

                                    data = Encoding.UTF8.GetString(buffer[readIndex..(readIndex + dataLength)]);
                                    readIndex += dataLength;

                                    if (client != null)
                                    {
                                        if (dataType != 'O')
                                            Process(client, code, (string)data);
                                        else
                                        {
                                            JObject.Parse((string)data).TryGetValue("Data", out JToken dataToken);
                                            JObject.Parse((string)data).TryGetValue("TypeName", out JToken typeNameToken);

                                            if (dataToken != null && typeNameToken != null)
                                                Process(client, code, dataToken.ToString(), typeNameToken.ToString());
                                        }
                                    }
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
        private bool IsConnected(User user)
        {
            try
            {
                if (user?.TCPClient != null && user.TCPClient.Client?.Connected == true)
                {
                    if (user.TCPClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (user.TCPClient.Client.Receive(buff, SocketFlags.Peek) == 0)
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
        private async Task CheckInClients()
        {
            while (true)
            {
                foreach (User user in users.Values)
                {
                    if (!IsConnected(user))
                    {
                        users.Remove(user.ConnectionID);
                        Disconnected(user);
                        Console.WriteLine($"A client has been disconnected {user}.");
                    }

                    await Task.Delay(25);
                }

                await Task.Delay(250);
            }
        }

        public virtual void Process(User user, int code, char data) { }
        public virtual void Process(User user, int code, int data) { ProcessDefault(user, code, data); }
        public virtual void Process(User user, int code, float data) { }
        public virtual void Process(User user, int code, double data) { }
        public virtual void Process(User user, int code, string data) { }
        public virtual void Process(User user, int code, string dataJSON, string typeName) { }
        public virtual void Process(User user, int code) { }
        private void ProcessDefault(User user, int code, int data)
        {
            if (data == int.MaxValue)
            {
                switch (code)
                {
                    case 'D':
                        user.Disconnect();
                        break;
                }
            }
        }

        public void Send(User user, char code)
        {
            Task.Run(async () =>
            {
                await user.TCPClient.GetStream().WriteAsync(Encoding.ASCII.GetBytes($"{code}0"));
            });
        }
        public void Send(User user, char code, string data)
        {
            Task.Run(async () =>
            {
                await user.TCPClient.GetStream().WriteAsync(Encoding.ASCII.GetBytes($"{code}S{(char)data.Length}").Concat(Encoding.UTF8.GetBytes($"{data}")).ToArray());
            });
        }
        public void Send(User user, char code, char data)
        {
            Task.Run(async () =>
            {
                await user.TCPClient.GetStream().WriteAsync(Encoding.ASCII.GetBytes($"{code}C").Concat(BitConverter.GetBytes(data)).ToArray());
            });
        }
        public void Send(User user, char code, int data)
        {
            Task.Run(async () =>
            {
                await user.TCPClient.GetStream().WriteAsync(Encoding.ASCII.GetBytes($"{code}I").Concat(BitConverter.GetBytes(data)).ToArray());
            });
        }
        public void Send(User user, char code, double data)
        {
            Task.Run(async () =>
            {
                await user.TCPClient.GetStream().WriteAsync(Encoding.ASCII.GetBytes($"{code}D").Concat(BitConverter.GetBytes(data)).ToArray());
            });
        }
        public void Send(User user, char code, float data)
        {
            Task.Run(async () =>
            {
                await user.TCPClient.GetStream().WriteAsync(Encoding.UTF8.GetBytes($"{code}F").Concat(BitConverter.GetBytes(data)).ToArray());
            });
        }
        public void Send<T>(User user, char code, T data)
        {
            Task.Run(async () =>
            {
                var jsonMessage = new JSONMessage<T>(data);
                await user.TCPClient.GetStream().WriteAsync(Encoding.ASCII.GetBytes($"{code}O{(char)jsonMessage.ToString().Length}").Concat(Encoding.UTF8.GetBytes($"{jsonMessage}")).ToArray());
            });
        }

        public T ConvertJSONTo<T>(string dataJSON)
        {
            return JsonSerializer.Deserialize<T>(dataJSON);
        }

        public virtual void Connected(User user)
        {
        }
        public virtual void Disconnected(User user)
        {
        }

        public override void Draw()
        {

        }
        public override void Loop()
        {

        }

        public List<User> Users { get => users.Values.ToList(); }
        public TcpListener Listener { get => listener; }
    }
}
