using SFML.Window;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

            bool stopSearching = false;
            while (!stopSearching)
            {
                stopSearching = true;
                try
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse(SERVER_HOST), SERVER_PORT));
                    Send('S');
                }
                catch
                {
                    stopSearching = false;
                }
            }

            stream = client.GetStream();
        }

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
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"{code}{(char)data.Length}").Concat(Encoding.UTF8.GetBytes($"{data}")).ToArray());
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

        public override void Draw()
        {

        }
        public override void Loop() { }

        public const int SERVER_PORT = 64198;
        public const string SERVER_HOST = "127.0.0.1";
    }
}
