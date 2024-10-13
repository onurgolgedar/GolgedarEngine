using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace GolgedarEngine
{
    public class Client : GameObject
    {
        TcpClient client;
        NetworkStream stream;

        public Client() : base(canCollide: false)
        {
            client = new TcpClient()
            {
                SendTimeout = 2000
            };

            Connect();
            CheckIn();
        }

        private void HandleClientAsync()
        {
            TcpClient tcpClient = client;

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
                                    data = BitConverter.ToChar(buffer.AsSpan()[readIndex..(readIndex + 2)]);
                                    readIndex += 2;
                                    if (client != null)
                                        Process(code, (char)data);
                                    break;

                                case 'I':
                                    data = BitConverter.ToInt32(buffer.AsSpan()[readIndex..(readIndex + 4)]);
                                    readIndex += 4;
                                    if (client != null)
                                        Process(code, (int)data);
                                    break;

                                case 'F':
                                    data = BitConverter.ToSingle(buffer.AsSpan()[readIndex..(readIndex + 4)]);
                                    readIndex += 4;
                                    if (client != null)
                                        Process(code, (float)data);
                                    break;

                                case 'D':
                                    data = BitConverter.ToDouble(buffer.AsSpan()[readIndex..(readIndex + 8)]);
                                    readIndex += 8;
                                    if (client != null)
                                        Process(code, (double)data);
                                    break;

                                case '0':
                                    if (client != null)
                                        Process(code);
                                    break;

                                default:
                                    int dataLength = Convert.ToInt32(buffer[readIndex]);
                                    readIndex++;

                                    data = Encoding.UTF8.GetString(buffer[readIndex..(readIndex + dataLength)]);
                                    readIndex += dataLength;

                                    if (client != null)
                                    {
                                        if (dataType != 'O')
                                            Process(code, (string)data);
                                        else
                                        {
                                            JObject.Parse((string)data).TryGetValue("Data", out JToken dataToken);
                                            JObject.Parse((string)data).TryGetValue("TypeName", out JToken typeNameToken);

                                            if (dataToken != null && typeNameToken != null)
                                                Process(code, dataToken.ToString(), typeNameToken.ToString());
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
        private async Task CheckIn()
        {
            while (true)
            {
                if (!IsConnected() && client != null)
                {
                    client = null;
                    stream = null;
                    Disconnected();
                }

                await Task.Delay(250);
            }
        }
        public bool IsConnected()
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
        public void Disconnect()
        {
            Send('C', int.MaxValue);

            client = null;
            stream = null;
            Disconnected();
        }
        public void Connect()
        {
            bool stopSearching = false;
            while (!stopSearching)
            {
                stopSearching = true;
                try
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse(SERVER_HOST), SERVER_PORT));
                    Connected();
                }
                catch
                {
                    stopSearching = false;
                }
            }

            stream = client.GetStream();
            Task.Run(HandleClientAsync);
        }

        public virtual void Process(int code, char data) { }
        public virtual void Process(int code, int data) { }
        public virtual void Process(int code, float data) { }
        public virtual void Process(int code, double data) { }
        public virtual void Process(int code, string data) { }
        public virtual void Process(int code, string dataJSON, string typeName) { }
        public virtual void Process(int code) { }

        public void Send(char code)
        {
            Task.Run(async () =>
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"{code}0"));
            });
        }
        public void Send(char code, string data)
        {
            Task.Run(async () =>
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"{code}S{(char)data.Length}").Concat(Encoding.UTF8.GetBytes($"{data}")).ToArray());
            });
        }
        public void Send(char code, char data)
        {
            Task.Run(async () =>
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"{code}C").Concat(BitConverter.GetBytes(data)).ToArray());
            });
        }
        public void Send(char code, int data)
        {
            Task.Run(async () =>
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"{code}I").Concat(BitConverter.GetBytes(data)).ToArray());
            });
        }
        public void Send(char code, double data)
        {
            Task.Run(async () =>
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"{code}D").Concat(BitConverter.GetBytes(data)).ToArray());
            });
        }
        public void Send(char code, float data)
        {
            Task.Run(async () =>
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes($"{code}F").Concat(BitConverter.GetBytes(data)).ToArray());
            });
        }
        public void Send<T>(char code, T data)
        {
            Task.Run(async () =>
            {
                var jsonMessage = new JSONMessage<T>(data);
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"{code}O{(char)jsonMessage.ToString().Length}").Concat(Encoding.UTF8.GetBytes($"{jsonMessage}")).ToArray());
            });
        }

        public T ConvertJSONTo<T>(string dataJSON)
        {
            return JsonSerializer.Deserialize<T>(dataJSON);
        }

        public virtual void Connected()
        {
        }
        public virtual void Disconnected()
        {
        }

        public override void Draw() { }
        public override void Loop() { }

        public const int SERVER_PORT = 64198;
        public const string SERVER_HOST = "127.0.0.1";
    }
}
